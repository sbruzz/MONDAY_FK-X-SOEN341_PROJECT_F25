using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages.Organizer
{
    public class QRScannerModel : PageModel
    {
        private readonly AppDbContext _context;

        public QRScannerModel(AppDbContext context)
        {
            _context = context;
        }

        public Event? Event { get; set; }
        public Ticket? ValidatedTicket { get; set; }
        public List<Ticket> RecentCheckIns { get; set; } = new();

        public int CheckedInCount { get; set; }

        [TempData]
        public string? ResultMessage { get; set; }

        [TempData]
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Load event
            Event = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (Event == null)
            {
                return Page();
            }

            // Verify ownership
            if (Event.OrganizerId != userId.Value)
            {
                return RedirectToPage("/Organizer/Events");
            }

            await LoadEventStats(eventId);

            return Page();
        }

        public async Task<IActionResult> OnPostScanQRAsync(int eventId, IFormFile qrImage)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Load event
            Event = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (Event == null || Event.OrganizerId != userId.Value)
            {
                return RedirectToPage("/Organizer/Events");
            }

            if (qrImage == null || qrImage.Length == 0)
            {
                ResultMessage = "Please upload a QR code image.";
                IsSuccess = false;
                await LoadEventStats(eventId);
                return Page();
            }

            // For prototype purposes, we'll use a simplified approach
            // In a real app, you would decode the QR code from the image
            // For now, we'll prompt for manual code entry
            ResultMessage = "QR code upload received. For this prototype, please use manual code entry below or provide the ticket unique code.";
            IsSuccess = false;

            await LoadEventStats(eventId);
            return Page();
        }

        public async Task<IActionResult> OnPostValidateCodeAsync(int eventId, string ticketCode)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Load event
            Event = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (Event == null || Event.OrganizerId != userId.Value)
            {
                return RedirectToPage("/Organizer/Events");
            }

            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                ResultMessage = "Please enter a ticket code.";
                IsSuccess = false;
                await LoadEventStats(eventId);
                return Page();
            }

            // Find ticket by unique code
            ValidatedTicket = await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.UniqueCode == ticketCode.Trim());

            if (ValidatedTicket == null)
            {
                ResultMessage = "Invalid ticket code. Ticket not found.";
                IsSuccess = false;
                await LoadEventStats(eventId);
                return Page();
            }

            // Verify ticket is for this event
            if (ValidatedTicket.EventId != eventId)
            {
                ResultMessage = $"This ticket is for a different event: {ValidatedTicket.Event.Title}";
                IsSuccess = false;
                await LoadEventStats(eventId);
                return Page();
            }

            // Check if already redeemed
            if (ValidatedTicket.IsRedeemed)
            {
                ResultMessage = $"Ticket Already Used! This ticket was checked in on {ValidatedTicket.RedeemedAt?.ToString("MMM dd, yyyy h:mm tt")}.";
                IsSuccess = false;
                await LoadEventStats(eventId);
                return Page();
            }

            // Mark ticket as redeemed
            ValidatedTicket.IsRedeemed = true;
            ValidatedTicket.RedeemedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            ResultMessage = "✓ Ticket Valid! Attendee Successfully Checked In.";
            IsSuccess = true;

            await LoadEventStats(eventId);
            return Page();
        }

        private async Task LoadEventStats(int eventId)
        {
            // Get check-in count
            CheckedInCount = await _context.Tickets
                .CountAsync(t => t.EventId == eventId && t.IsRedeemed);

            // Get recent check-ins
            RecentCheckIns = await _context.Tickets
                .Include(t => t.User)
                .Where(t => t.EventId == eventId && t.IsRedeemed)
                .OrderByDescending(t => t.RedeemedAt)
                .Take(10)
                .ToListAsync();
        }
    }
}
