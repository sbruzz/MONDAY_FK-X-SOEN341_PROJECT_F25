using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages;

public class EventsModel : PageModel
{
    private readonly AppDbContext _context;

    public EventsModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Event> Events { get; set; } = new();
    public List<Organization> Organizations { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? OrganizationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string DateFilter { get; set; } = "all";

    public async Task OnGetAsync()
    {
        // Load organizations for filter dropdown
        Organizations = await _context.Organizations
            .OrderBy(o => o.Name)
            .ToListAsync();

        // Build query for events (only approved events for public view)
        var query = _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Organizer)
            .Where(e => e.ApprovalStatus == ApprovalStatus.Approved)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(e =>
                e.Title.Contains(SearchTerm) ||
                e.Description.Contains(SearchTerm) ||
                e.Location.Contains(SearchTerm));
        }

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(Category) && Category != "All")
        {
            query = query.Where(e => e.Category == Category);
        }

        // Apply organization filter
        if (OrganizationId.HasValue)
        {
            query = query.Where(e => e.OrganizationId == OrganizationId.Value);
        }

        // Apply date filter
        var now = DateTime.UtcNow;
        switch (DateFilter)
        {
            case "today":
                var today = now.Date;
                query = query.Where(e => e.EventDate.Date == today);
                break;
            case "week":
                var weekEnd = now.AddDays(7);
                query = query.Where(e => e.EventDate >= now && e.EventDate <= weekEnd);
                break;
            case "month":
                var monthEnd = now.AddMonths(1);
                query = query.Where(e => e.EventDate >= now && e.EventDate <= monthEnd);
                break;
            default: // "all"
                query = query.Where(e => e.EventDate >= now); // Only future events
                break;
        }

        // Order by date and load events
        Events = await query
            .OrderBy(e => e.EventDate)
            .ToListAsync();
    }
}
