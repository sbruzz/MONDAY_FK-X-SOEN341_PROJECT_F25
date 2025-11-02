using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages.Student
{
    public class SavedModel : PageModel
    {
        private readonly AppDbContext _context;

        public SavedModel(AppDbContext context)
        {
            _context = context;
        }

        public List<SavedEvent> SavedEvents { get; set; } = new();
        public HashSet<int> TicketEventIds { get; set; } = new();
        public int UpcomingCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string View { get; set; } = "upcoming";

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Load all saved events with event details
            var allSavedEvents = await _context.SavedEvents
                .Include(se => se.Event)
                    .ThenInclude(e => e.Organization)
                .Where(se => se.UserId == userId.Value && se.Event.ApprovalStatus == ApprovalStatus.Approved)
                .OrderBy(se => se.Event.EventDate)
                .ToListAsync();

            var now = DateTime.UtcNow;

            // Calculate upcoming count
            UpcomingCount = allSavedEvents.Count(se => se.Event.EventDate >= now);

            // Apply view filter
            SavedEvents = View switch
            {
                "upcoming" => allSavedEvents.Where(se => se.Event.EventDate >= now).ToList(),
                _ => allSavedEvents
            };

            // Get list of events user has tickets for
            var ticketEventIds = await _context.Tickets
                .Where(t => t.UserId == userId.Value)
                .Select(t => t.EventId)
                .ToListAsync();
            TicketEventIds = new HashSet<int>(ticketEventIds);

            return Page();
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

            return RedirectToPage(new { view = View });
        }
    }
}
