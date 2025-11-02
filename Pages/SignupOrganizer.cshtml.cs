using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using BCrypt.Net;

namespace CampusEvents.Pages;

public class SignupOrganizerModel : PageModel
{
    private readonly AppDbContext _context;

    public SignupOrganizerModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public int? OrganizationId { get; set; }

    [BindProperty]
    public string? NewOrganizationName { get; set; }

    [BindProperty]
    public string? NewOrganizationDescription { get; set; }

    [BindProperty]
    public string Position { get; set; } = string.Empty;

    [BindProperty]
    public string Department { get; set; } = string.Empty;

    [BindProperty]
    public string PhoneNumber { get; set; } = string.Empty;

    public List<Organization> Organizations { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        Organizations = await _context.Organizations.OrderBy(o => o.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Organizations = await _context.Organizations.OrderBy(o => o.Name).ToListAsync();

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == Email))
        {
            ErrorMessage = "An account with this email already exists.";
            return Page();
        }

        // Handle new organization creation
        int finalOrganizationId;
        if (OrganizationId == null || OrganizationId == 0)
        {
            if (string.IsNullOrWhiteSpace(NewOrganizationName))
            {
                ErrorMessage = "Please select an existing organization or create a new one.";
                return Page();
            }

            // Create new organization
            var newOrg = new Organization
            {
                Name = NewOrganizationName,
                Description = NewOrganizationDescription ?? string.Empty
            };
            _context.Organizations.Add(newOrg);
            await _context.SaveChangesAsync();
            finalOrganizationId = newOrg.Id;
        }
        else
        {
            finalOrganizationId = OrganizationId.Value;
        }

        // Create new organizer user
        var user = new User
        {
            Name = Name,
            Email = Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
            Role = UserRole.Organizer,
            ApprovalStatus = ApprovalStatus.Pending, // Organizers need approval
            OrganizationId = finalOrganizationId,
            Position = Position,
            Department = Department,
            PhoneNumber = PhoneNumber
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Redirect to a pending approval page or login
        TempData["Message"] = "Your organizer account has been created! Please wait for admin approval before you can log in.";
        return RedirectToPage("/Login");
    }
}
