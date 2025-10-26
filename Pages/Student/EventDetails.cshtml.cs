using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using QRCoder;

namespace CampusEvents.Pages.Student
{
    public class EventDetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public EventDetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Event? Event { get; set; }
        public bool HasTicket { get; set; }
        public bool IsSaved { get; set; }

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public bool IsSuccess { get; set; }

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

            // Check if user has ticket
            HasTicket = await _context.Tickets
                .AnyAsync(t => t.EventId == id && t.UserId == userId.Value);

            // Check if event is saved
            IsSaved = await _context.SavedEvents
                .AnyAsync(se => se.EventId == id && se.UserId == userId.Value);

            return Page();
        }

        public async Task<IActionResult> OnPostClaimTicketAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Reload event
            Event = await _context.Events
                .Include(e => e.Organization)
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (Event == null)
            {
                Message = "Event not found.";
                IsSuccess = false;
                return RedirectToPage(new { id });
            }

            // Check if user already has a ticket
            var existingTicket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.EventId == id && t.UserId == userId.Value);

            if (existingTicket != null)
            {
                Message = "You already have a ticket for this event!";
                IsSuccess = false;
                return RedirectToPage(new { id });
            }

            // Check capacity
            if (Event.TicketsIssued >= Event.Capacity)
            {
                Message = "Sorry, this event is full.";
                IsSuccess = false;
                return RedirectToPage(new { id });
            }

            // Generate unique code for ticket
            var uniqueCode = Guid.NewGuid().ToString();

            // Generate QR Code
            string qrCodeBase64 = GenerateQRCode(uniqueCode);

            // Create ticket
            var ticket = new Ticket
            {
                EventId = id,
                UserId = userId.Value,
                UniqueCode = uniqueCode,
                QrCodeImage = qrCodeBase64,
                ClaimedAt = DateTime.UtcNow,
                IsRedeemed = false
            };

            _context.Tickets.Add(ticket);

            // Update event tickets issued count
            Event.TicketsIssued++;

            await _context.SaveChangesAsync();

            Message = Event.Price == 0
                ? "Ticket claimed successfully!"
                : $"Ticket purchased successfully! (Mock payment of ${Event.Price:F2} processed)";
            IsSuccess = true;

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostSaveEventAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            var existingSave = await _context.SavedEvents
                .FirstOrDefaultAsync(se => se.UserId == userId.Value && se.EventId == id);

            if (existingSave == null)
            {
                var savedEvent = new SavedEvent
                {
                    UserId = userId.Value,
                    EventId = id,
                    SavedAt = DateTime.UtcNow
                };

                _context.SavedEvents.Add(savedEvent);
                await _context.SaveChangesAsync();

                Message = "Event saved to your calendar!";
                IsSuccess = true;
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostUnsaveEventAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            var savedEvent = await _context.SavedEvents
                .FirstOrDefaultAsync(se => se.UserId == userId.Value && se.EventId == id);

            if (savedEvent != null)
            {
                _context.SavedEvents.Remove(savedEvent);
                await _context.SaveChangesAsync();

                Message = "Event removed from your calendar.";
                IsSuccess = true;
            }

            return RedirectToPage(new { id });
        }

        private string GenerateQRCode(string text)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                using (BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData))
                {
                    byte[] qrCodeBytes = qrCode.GetGraphic(20);
                    return Convert.ToBase64String(qrCodeBytes);
                }
            }
        }
    }
}
