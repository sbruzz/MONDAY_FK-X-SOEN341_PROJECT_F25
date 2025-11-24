using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using BCrypt.Net;

namespace CampusEvents.Pages;

public class LoginModel : PageModel
{
    private readonly AppDbContext _context;

    public LoginModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string Role { get; set; } = "Student";

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostLoginAsync()
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
        {
            TempData["Error"] = "Invalid email or password";
            return Page();
        }

        if (user.Role.ToString() != roletab)
        {
            TempData["Error"] = "Invalid Role";
            return Page();
        }

        if (user.ApprovalStatus == ApprovalStatus.Rejected)
        {
            TempData["Error"] = "Your account has been rejected. Please contact the administrator.";
            return Page();
        }

        // Check if organizer/admin needs approval
        if ((user.Role == UserRole.Organizer || user.Role == UserRole.Admin)
            && user.ApprovalStatus != ApprovalStatus.Approved)
        {
            TempData["Error"] = "Your account is pending approval";
            return Page();
        }

        // Store user ID in session (simplified - in production use proper authentication)
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserRole", user.Role.ToString());

        // Redirect based on role
        return user.Role switch
        {
            UserRole.Student => RedirectToPage("/Student/Home"),
            UserRole.Organizer => RedirectToPage("/Organizer/Home"),
            UserRole.Admin => RedirectToPage("/Admin/Home"),
            _ => RedirectToPage("/Index")
        };
    }

    public async Task<IActionResult> OnPostSignupAsync()
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == Email))
        {
            TempData["Error"] = "Email already registered";
            return Page();
        }

        // Parse role from string
        if (!Enum.TryParse<UserRole>(Role, out var userRole))
        {
            TempData["Error"] = "Invalid role selected";
            return Page();
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(Password);

        // Create user
        var user = new User
        {
            Email = Email,
            PasswordHash = passwordHash,
            Name = Name,
            Role = userRole,
            ApprovalStatus = userRole == UserRole.Student ? ApprovalStatus.Approved : ApprovalStatus.Pending
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        if (userRole == UserRole.Student)
        {
            // Auto-login students
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());
            TempData["Message"] = "Account created successfully!";
            return RedirectToPage("/Student/Home");
        }
        else
        {
            TempData["Message"] = "Account created! Awaiting admin approval.";
            return Page();
        }
    }
}
