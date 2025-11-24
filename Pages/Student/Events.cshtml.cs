using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages.Student
{
    public class EventsModel : PageModel
    {
        private readonly AppDbContext _context;

        public EventsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Event> Events { get; set; } = new();
        public List<Organization> Organizations { get; set; } = new();
        public HashSet<int> SavedEventIds { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public EventCategory? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? OrganizationId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string DateFilter { get; set; } = "all";

        [BindProperty(SupportsGet = true)]
        public DateTime StartTime { get; set; } = new DateTime(2020, 1, 1);

        [BindProperty(SupportsGet = true)]
        public DateTime EndTime { get; set; } = new DateTime(2030, 1, 1);

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current user ID from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Load organizations for filter dropdown
            Organizations = await _context.Organizations
                .OrderBy(o => o.Name)
                .ToListAsync();

            // Get user's saved events
            var savedEvents = await _context.SavedEvents
                .Where(se => se.UserId == userId.Value)
                .Select(se => se.EventId)
                .ToListAsync();
            SavedEventIds = new HashSet<int>(savedEvents);

            // Build query for events
            var query = _context.Events
                .Include(e => e.Organization)
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
            if (Category.HasValue)
            {
                query = query.Where(e => e.Category == Category.Value);
            }

            // Apply organization filter
            if (OrganizationId.HasValue)
            {
                query = query.Where(e => e.OrganizationId == OrganizationId.Value);
            }

            // Apply date filter
            query = query.Where(e => e.EventDate.Date >= StartTime && e.EventDate.Date <= EndTime);

            // Order by date and load events
            Events = await query
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostSaveEventAsync(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Check if already saved
            var existingSave = await _context.SavedEvents
                .FirstOrDefaultAsync(se => se.UserId == userId.Value && se.EventId == eventId);

            if (existingSave == null)
            {
                var savedEvent = new SavedEvent
                {
                    UserId = userId.Value,
                    EventId = eventId,
                    SavedAt = DateTime.UtcNow
                };

                _context.SavedEvents.Add(savedEvent);
                await _context.SaveChangesAsync();
            }

            // Redirect back to the same page with filters preserved
            return RedirectToPage(new
            {
                searchTerm = SearchTerm,
                category = Category,
                organization = OrganizationId,
                dateFilter = DateFilter
            });
        }

        public async Task<IActionResult> OnPostUnsaveEventAsync(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            var savedEvent = await _context.SavedEvents
                .FirstOrDefaultAsync(se => se.UserId == userId.Value && se.EventId == eventId);

            if (savedEvent != null)
            {
                _context.SavedEvents.Remove(savedEvent);
                await _context.SaveChangesAsync();
            }

            // Redirect back to the same page with filters preserved
            return RedirectToPage(new
            {
                searchTerm = SearchTerm,
                category = Category,
                organization = OrganizationId,
                dateFilter = DateFilter
            });
        }
    }
}
