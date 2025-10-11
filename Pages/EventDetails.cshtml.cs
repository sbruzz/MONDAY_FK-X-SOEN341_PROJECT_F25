/*
 * Page for each event that creates a tickets for students and saves them to db
 */

using CampusEvents.Data;
using CampusEvents.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CampusEvents.Pages
{
    public class EventDetailsModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public EventDetailsModel(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        public Event Event { get; set; } = null!;
        public List<Ticket> Tickets { get; set; } = new List<Ticket>();

        public async Task<IActionResult> OnGetAsync()
        {
            Event = await _context.Events.FirstOrDefaultAsync(e => e.Id == EventId);
            if (Event == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var currentUserId = user.Id;

            Tickets = await _context.Tickets
                .Where(t => t.EventId == EventId && t.UserId == currentUserId)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostClaimTicketAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var currentUserId = user.Id;

            // Check if ticket already exists
            if (_context.Tickets.Any(t => t.EventId == EventId && t.UserId == currentUserId))
            {
                TempData["Message"] = "You already have a ticket for this event.";
                return RedirectToPage(new { EventId });
            }

            var ticket = new Ticket
            {
                EventId = EventId,
                UserId = currentUserId,
                UniqueCode = Guid.NewGuid().ToString("N"),
                ClaimedAt = DateTime.UtcNow,
                IsRedeemed = false
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Ticket created! Code: {ticket.UniqueCode}";
            return RedirectToPage(new { EventId });
        }
    }
}
