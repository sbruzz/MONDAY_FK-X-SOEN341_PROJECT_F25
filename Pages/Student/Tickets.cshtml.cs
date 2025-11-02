using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages.Student
{
    public class TicketsModel : PageModel
    {
        private readonly AppDbContext _context;

        public TicketsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Ticket> Tickets { get; set; } = new();
        public int UpcomingCount { get; set; }
        public int PastCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } = "upcoming";

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Load all user's tickets with event details
            var allTickets = await _context.Tickets
                .Include(t => t.Event)
                    .ThenInclude(e => e.Organization)
                .Where(t => t.UserId == userId.Value)
                .OrderByDescending(t => t.Event.EventDate)
                .ToListAsync();

            var now = DateTime.UtcNow;

            // Calculate counts
            UpcomingCount = allTickets.Count(t => t.Event.EventDate >= now);
            PastCount = allTickets.Count(t => t.Event.EventDate < now);

            // Apply filter
            Tickets = Filter switch
            {
                "upcoming" => allTickets.Where(t => t.Event.EventDate >= now).ToList(),
                "past" => allTickets.Where(t => t.Event.EventDate < now).ToList(),
                _ => allTickets
            };

            return Page();
        }
    }
}
