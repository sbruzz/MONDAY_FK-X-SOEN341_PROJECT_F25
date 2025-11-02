using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages.Admin;

public class EditUserModel : PageModel
{
    private readonly AppDbContext _context;

    public EditUserModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public new User User { get; set; } = null!;

    public List<Organization> Organizations { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        User = user;
        Organizations = await _context.Organizations.OrderBy(o => o.Name).ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Organizations = await _context.Organizations.OrderBy(o => o.Name).ToListAsync();
            return Page();
        }

        var userToUpdate = await _context.Users.FindAsync(User.Id);
        if (userToUpdate == null)
        {
            Message = "User not found.";
            IsSuccess = false;
            return RedirectToPage("/Admin/Users");
        }

        // Update basic info
        userToUpdate.Name = User.Name;
        userToUpdate.Email = User.Email;
        userToUpdate.Role = User.Role;
        userToUpdate.ApprovalStatus = User.ApprovalStatus;

        // Update student fields
        if (User.Role == UserRole.Student)
        {
            userToUpdate.StudentId = User.StudentId;
            userToUpdate.Program = User.Program;
            userToUpdate.YearOfStudy = User.YearOfStudy;
            userToUpdate.PhoneNumber = User.PhoneNumber;
            userToUpdate.OrganizationId = null;
            userToUpdate.Position = null;
            userToUpdate.Department = null;
        }
        // Update organizer fields
        else if (User.Role == UserRole.Organizer)
        {
            userToUpdate.OrganizationId = User.OrganizationId;
            userToUpdate.Position = User.Position;
            userToUpdate.Department = User.Department;
            userToUpdate.PhoneNumber = User.PhoneNumber;
            userToUpdate.StudentId = null;
            userToUpdate.Program = null;
            userToUpdate.YearOfStudy = null;
        }
        // Clear both if admin
        else
        {
            userToUpdate.StudentId = null;
            userToUpdate.Program = null;
            userToUpdate.YearOfStudy = null;
            userToUpdate.PhoneNumber = null;
            userToUpdate.OrganizationId = null;
            userToUpdate.Position = null;
            userToUpdate.Department = null;
        }

        await _context.SaveChangesAsync();

        Message = $"User {userToUpdate.Name} has been updated successfully.";
        IsSuccess = true;
        return RedirectToPage("/Admin/Users");
    }
}
