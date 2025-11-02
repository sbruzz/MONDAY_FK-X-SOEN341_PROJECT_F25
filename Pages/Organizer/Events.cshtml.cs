using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages.Organizer
{
    public class EventsModel : PageModel
    {
        private readonly AppDbContext _context;

        public EventsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Event> Events { get; set; } = new();

        public int TotalEvents { get; set; }
        public int PendingEvents { get; set; }
        public int ApprovedEvents { get; set; }
        public int RejectedEvents { get; set; }
        public int TotalTicketsIssued { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "all";

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Verify user is an organizer
            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null || user.Role != UserRole.Organizer)
            {
                return RedirectToPage("/Index");
            }

            // Load all organizer's events
            var allEvents = await _context.Events
                .Include(e => e.Organization)
                .Where(e => e.OrganizerId == userId.Value)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            // Calculate summary stats
            TotalEvents = allEvents.Count;
            PendingEvents = allEvents.Count(e => e.ApprovalStatus == ApprovalStatus.Pending);
            ApprovedEvents = allEvents.Count(e => e.ApprovalStatus == ApprovalStatus.Approved);
            RejectedEvents = allEvents.Count(e => e.ApprovalStatus == ApprovalStatus.Rejected);
            TotalTicketsIssued = allEvents.Sum(e => e.TicketsIssued);

            // Apply filter
            Events = StatusFilter switch
            {
                "pending" => allEvents.Where(e => e.ApprovalStatus == ApprovalStatus.Pending).ToList(),
                "approved" => allEvents.Where(e => e.ApprovalStatus == ApprovalStatus.Approved).ToList(),
                "rejected" => allEvents.Where(e => e.ApprovalStatus == ApprovalStatus.Rejected).ToList(),
                _ => allEvents
            };

            return Page();
        }
    }
}
