using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using System.Text;

namespace CampusEvents.Pages.Organizer
{
    public class EventDetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public EventDetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Event? Event { get; set; }
        public List<Ticket> RecentAttendees { get; set; } = new();

        public int RedeemedTickets { get; set; }
        public int RemainingCapacity { get; set; }
        public double AttendanceRate { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Load event with related data
            Event = await _context.Events
                .Include(e => e.Organization)
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (Event == null)
            {
                return Page();
            }

            // Verify ownership
            if (Event.OrganizerId != userId.Value)
            {
                return RedirectToPage("/Organizer/Events");
            }

            // Load attendees
            RecentAttendees = await _context.Tickets
                .Include(t => t.User)
                .Where(t => t.EventId == id)
                .OrderByDescending(t => t.ClaimedAt)
                .ToListAsync();

            // Calculate stats
            RedeemedTickets = RecentAttendees.Count(t => t.IsRedeemed);
            RemainingCapacity = Event.Capacity - Event.TicketsIssued;
            AttendanceRate = Event.TicketsIssued > 0
                ? (RedeemedTickets * 100.0 / Event.TicketsIssued)
                : 0;

            return Page();
        }

        public async Task<IActionResult> OnPostExportCSVAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Load event
            var eventData = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventData == null || eventData.OrganizerId != userId.Value)
            {
                return RedirectToPage("/Organizer/Events");
            }

            // Load all tickets for this event
            var tickets = await _context.Tickets
                .Include(t => t.User)
                .Where(t => t.EventId == id)
                .OrderBy(t => t.ClaimedAt)
                .ToListAsync();

            // Generate CSV content
            var csv = new StringBuilder();

            // CSV Header
            csv.AppendLine("Ticket ID,User Name,Email,Claimed At,Redeemed,Redeemed At,Unique Code");

            // CSV Rows
            foreach (var ticket in tickets)
            {
                csv.AppendLine($"{ticket.Id}," +
                              $"\"{ticket.User.Name}\"," +
                              $"\"{ticket.User.Email}\"," +
                              $"\"{ticket.ClaimedAt.ToString("yyyy-MM-dd HH:mm:ss")}\"," +
                              $"{(ticket.IsRedeemed ? "Yes" : "No")}," +
                              $"\"{(ticket.RedeemedAt.HasValue ? ticket.RedeemedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "N/A")}\"," +
                              $"\"{ticket.UniqueCode}\"");
            }

            // Return CSV file
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"{eventData.Title.Replace(" ", "_")}_Attendees_{DateTime.Now:yyyyMMdd}.csv";

            return File(bytes, "text/csv", fileName);
        }
    }
}
