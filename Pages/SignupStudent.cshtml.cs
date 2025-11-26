using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using BCrypt.Net;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace CampusEvents.Pages;

public class SignupStudentModel : PageModel
{
    private readonly AppDbContext _context;

    public SignupStudentModel(AppDbContext context)
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
    public string StudentId { get; set; } = string.Empty;

    [BindProperty]
    public string Program { get; set; } = string.Empty;

    [BindProperty]
    public string YearOfStudy { get; set; } = string.Empty;

    [BindProperty]
    public string PhoneNumber { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Check if email or student id already exists
        if (await _context.Users.AnyAsync(u => u.Email == Email || u.StudentId == StudentId))
        {
            ErrorMessage = "An account with this email or student id already exists.";
            return Page();
        }


        if (!IsValidEmail(Email))
        {
            ErrorMessage = "This is not a valid email address";
            return Page();            
        }
        
        if (!Regex.IsMatch(PhoneNumber, @"^\d{10}$"))
        {
            ErrorMessage = "This is not a valid phone number";
            return Page();
        }

            // Create new student user
            var user = new User
            {
                Name = Name,
                Email = Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
                Role = UserRole.Student,
                ApprovalStatus = ApprovalStatus.Approved, // Students are auto-approved
                StudentId = StudentId,
                Program = Program,
                YearOfStudy = YearOfStudy,
                PhoneNumber = PhoneNumber
            };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Log them in immediately
        HttpContext.Session.SetInt32("UserId", user.Id);

        return RedirectToPage("/Student/Home");
    }

    public bool IsValidEmail(string emailaddress)
    {
        try
        {
            MailAddress m = new MailAddress(emailaddress);

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
