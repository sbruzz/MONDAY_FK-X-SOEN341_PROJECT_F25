using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages.Admin;

public class AdminHomeModel : PageModel
{
    private readonly AppDbContext _context;

    public AdminHomeModel(AppDbContext context)
    {
        _context = context;
    }

    public string UserName { get; set; } = string.Empty;

    // Global Statistics
    public int TotalEvents { get; set; }
    public int TotalTicketsIssued { get; set; }
    public int TotalUsers { get; set; }
    public int TotalOrganizations { get; set; }
    public int PendingUsers { get; set; }
    public int PendingEvents { get; set; }

    // Trends
    public int EventsThisMonth { get; set; }
    public int TicketsThisMonth { get; set; }
    public double AverageAttendanceRate { get; set; }
    public int ActiveOrganizers { get; set; }

    public List<CategoryStat> EventsByCategory { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            UserName = user.Name;
        }

        // Load global statistics
        TotalEvents = await _context.Events.CountAsync();
        TotalTicketsIssued = await _context.Events.SumAsync(e => e.TicketsIssued);
        TotalUsers = await _context.Users.CountAsync();
        TotalOrganizations = await _context.Organizations.CountAsync();

        PendingUsers = await _context.Users
            .Where(u => u.ApprovalStatus == ApprovalStatus.Pending)
            .CountAsync();

        PendingEvents = await _context.Events
            .Where(e => e.ApprovalStatus == ApprovalStatus.Pending)
            .CountAsync();

        // This month stats
        var thisMonth = DateTime.UtcNow.AddMonths(-1);
        EventsThisMonth = await _context.Events
            .Where(e => e.CreatedAt >= thisMonth)
            .CountAsync();

        TicketsThisMonth = await _context.Tickets
            .Where(t => t.ClaimedAt >= thisMonth)
            .CountAsync();

        // Average attendance rate
        var totalTickets = await _context.Tickets.CountAsync();
        var redeemedTickets = await _context.Tickets.Where(t => t.IsRedeemed).CountAsync();
        AverageAttendanceRate = totalTickets > 0 ? (redeemedTickets * 100.0 / totalTickets) : 0;

        // Active organizers
        ActiveOrganizers = await _context.Users
            .Where(u => u.Role == UserRole.Organizer && u.ApprovalStatus == ApprovalStatus.Approved)
            .CountAsync();

        // Events by category
        EventsByCategory = await _context.Events
            .GroupBy(e => e.Category)
            .Select(g => new CategoryStat
            {
                Category = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(s => s.Count)
            .ToListAsync();

        return Page();
    }
}

public class CategoryStat
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}
