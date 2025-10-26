using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages.Admin;

public class UsersModel : PageModel
{
    private readonly AppDbContext _context;

    public UsersModel(AppDbContext context)
    {
        _context = context;
    }

    public List<User> Users { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? RoleFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

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

        // Load users with filters
        var query = _context.Users.AsQueryable();

        // Filter by role
        if (!string.IsNullOrWhiteSpace(RoleFilter) && RoleFilter != "All")
        {
            if (Enum.TryParse<UserRole>(RoleFilter, out var role))
            {
                query = query.Where(u => u.Role == role);
            }
        }

        // Filter by approval status
        if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "All")
        {
            if (Enum.TryParse<ApprovalStatus>(StatusFilter, out var status))
            {
                query = query.Where(u => u.ApprovalStatus == status);
            }
        }

        Users = await query
            .OrderBy(u => u.ApprovalStatus)
            .ThenBy(u => u.Role)
            .ThenBy(u => u.Name)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            Message = "User not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        user.ApprovalStatus = ApprovalStatus.Approved;
        await _context.SaveChangesAsync();

        Message = $"{user.Name} has been approved as {user.Role}.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            Message = "User not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        user.ApprovalStatus = ApprovalStatus.Rejected;
        await _context.SaveChangesAsync();

        Message = $"{user.Name}'s request has been rejected.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            Message = "User not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Don't allow deleting yourself
        var currentUserId = HttpContext.Session.GetInt32("UserId");
        if (user.Id == currentUserId)
        {
            Message = "You cannot delete your own account.";
            IsSuccess = false;
            return RedirectToPage();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        Message = $"User {user.Name} has been deleted.";
        IsSuccess = true;
        return RedirectToPage();
    }
}
