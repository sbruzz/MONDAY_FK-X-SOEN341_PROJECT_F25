using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AppDbContext _context;

    public IndexModel(ILogger<IndexModel> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public List<Event> FeaturedEvents { get; set; } = new();
    public List<Event> UpcomingEvents { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CategoryFilter { get; set; }

    public async Task OnGetAsync()
    {
        // Get featured events (next 3 events with most tickets issued)
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
        if (!string.IsNullOrWhiteSpace(CategoryFilter) && CategoryFilter != "All")
        {
            if (Enum.TryParse<EventCategory>(CategoryFilter, out var category))
            {
                query = query.Where(e => e.Category == category);
            }
        }

        // Get upcoming events
        UpcomingEvents = await query
            .OrderBy(e => e.EventDate)
            .Take(12)
            .ToListAsync();
    }
}
