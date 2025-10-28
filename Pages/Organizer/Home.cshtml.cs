using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages.Organizer;

public class OrganizerHomeModel : PageModel
{
    private readonly AppDbContext _context;

    public OrganizerHomeModel(AppDbContext context)
    {
        _context = context;
    }

    public string UserName { get; set; } = string.Empty;

    // Dashboard Statistics
    public int TotalEvents { get; set; }
    public int TotalTicketsSold { get; set; }
    public int UpcomingEventsCount { get; set; }
    public int TotalCheckIns { get; set; }
    public double AverageAttendanceRate { get; set; }
    public decimal TotalRevenue { get; set; }

    // Event Status Counts
    public int ApprovedEventsCount { get; set; }
    public int PendingEventsCount { get; set; }
    public int RejectedEventsCount { get; set; }

    // Upcoming Events
    public List<Event> UpcomingEvents { get; set; } = new();

    // Best Performing Event
    public Event? MostPopularEvent { get; set; }
    public int MostPopularEventTickets { get; set; }

    // Recent Activity
    public List<Ticket> RecentTickets { get; set; } = new();

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

        // Load all organizer's events
        var allEvents = await _context.Events
            .Include(e => e.Organization)
            .Where(e => e.OrganizerId == userId.Value)
            .ToListAsync();

        // Calculate statistics
        TotalEvents = allEvents.Count;
        TotalTicketsSold = allEvents.Sum(e => e.TicketsIssued);
        UpcomingEventsCount = allEvents.Count(e => e.EventDate > DateTime.UtcNow);

        // Event status breakdown
        ApprovedEventsCount = allEvents.Count(e => e.ApprovalStatus == ApprovalStatus.Approved);
        PendingEventsCount = allEvents.Count(e => e.ApprovalStatus == ApprovalStatus.Pending);
        RejectedEventsCount = allEvents.Count(e => e.ApprovalStatus == ApprovalStatus.Rejected);

        // Calculate total check-ins and attendance rate
        var eventIds = allEvents.Select(e => e.Id).ToList();
        var allTickets = await _context.Tickets
            .Where(t => eventIds.Contains(t.EventId))
            .ToListAsync();

        TotalCheckIns = allTickets.Count(t => t.IsRedeemed);
        AverageAttendanceRate = TotalTicketsSold > 0
            ? (TotalCheckIns * 100.0 / TotalTicketsSold)
            : 0;

        // Calculate total revenue
        TotalRevenue = allEvents
            .Where(e => e.Price > 0)
            .Sum(e => e.Price * e.TicketsIssued);

        // Get upcoming events (next 5)
        UpcomingEvents = allEvents
            .Where(e => e.EventDate > DateTime.UtcNow)
            .OrderBy(e => e.EventDate)
            .Take(5)
            .ToList();

        // Find most popular event (highest ticket sales)
        if (allEvents.Any())
        {
            MostPopularEvent = allEvents
                .OrderByDescending(e => e.TicketsIssued)
                .FirstOrDefault();
            MostPopularEventTickets = MostPopularEvent?.TicketsIssued ?? 0;
        }

        // Recent ticket activity (last 10)
        RecentTickets = await _context.Tickets
            .Include(t => t.Event)
            .Include(t => t.User)
            .Where(t => eventIds.Contains(t.EventId))
            .OrderByDescending(t => t.ClaimedAt)
            .Take(10)
            .ToListAsync();

        return Page();
    }
}
