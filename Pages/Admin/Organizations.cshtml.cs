using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages.Admin;

public class OrganizationsModel : PageModel
{
    private readonly AppDbContext _context;

    public OrganizationsModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Organization> Organizations { get; set; } = new();

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; }

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        Organizations = await _context.Organizations
            .Include(o => o.Events)
            .OrderBy(o => o.Name)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Message = "Organization name is required.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Check if organization already exists
        if (await _context.Organizations.AnyAsync(o => o.Name == Name))
        {
            Message = "An organization with this name already exists.";
            IsSuccess = false;
            return RedirectToPage();
        }

        var organization = new Organization
        {
            Name = Name,
            Description = Description ?? string.Empty
        };

        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        Message = $"Organization '{Name}' has been created successfully.";
        IsSuccess = true;

        // Clear form
        Name = string.Empty;
        Description = null;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var organization = await _context.Organizations
            .Include(o => o.Events)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization == null)
        {
            Message = "Organization not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Check if organization has events
        if (organization.Events.Any())
        {
            Message = $"Cannot delete '{organization.Name}' because it has {organization.Events.Count} associated events.";
            IsSuccess = false;
            return RedirectToPage();
        }

        _context.Organizations.Remove(organization);
        await _context.SaveChangesAsync();

        Message = $"Organization '{organization.Name}' has been deleted.";
        IsSuccess = true;
        return RedirectToPage();
    }
}
