using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages.Student;

public class StudentHomeModel : PageModel
{
    private readonly AppDbContext _context;

    public StudentHomeModel(AppDbContext context)
    {
        _context = context;
    }

    public string UserName { get; set; } = string.Empty;
    public List<Event> FeaturedEvents { get; set; } = new();
    public List<Event> UpcomingEvents { get; set; } = new();
    public int SavedEventsCount { get; set; }
    public int TicketsCount { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

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

        // Get user stats
        SavedEventsCount = await _context.SavedEvents.CountAsync(se => se.UserId == userId);
        TicketsCount = await _context.Tickets.CountAsync(t => t.UserId == userId);

        // Get featured events (most popular)
        FeaturedEvents = await _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Organizer)
            .Where(e => e.ApprovalStatus == ApprovalStatus.Approved && e.EventDate > DateTime.UtcNow)
            .OrderByDescending(e => e.TicketsIssued)
            .Take(3)
            .ToListAsync();

        // Build query for upcoming events with filters
        var query = _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Organizer)
            .Where(e => e.ApprovalStatus == ApprovalStatus.Approved && e.EventDate > DateTime.UtcNow)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(e => e.Title.Contains(SearchTerm) ||
                                     e.Description.Contains(SearchTerm) ||
                                     e.Location.Contains(SearchTerm));
        }

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(Category) && Category != "All")
        {
            query = query.Where(e => e.Category == Category);
        }

        // Get upcoming events
        UpcomingEvents = await query
            .OrderBy(e => e.EventDate)
            .Take(12)
            .ToListAsync();

        return Page();
    }
}
