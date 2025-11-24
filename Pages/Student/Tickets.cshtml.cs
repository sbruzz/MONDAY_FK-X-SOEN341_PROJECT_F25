using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Services;
using QRCoder;

namespace CampusEvents.Pages.Student
{
    public class TicketsModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly TicketSigningService _signingService;

        public TicketsModel(AppDbContext context, TicketSigningService signingService)
        {
            _context = context;
            _signingService = signingService;
        }

        public List<Ticket> Tickets { get; set; } = new();
        public Dictionary<int, string> TicketQRCodes { get; set; } = new();
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

            // Generate signed QR codes once per request in the page model (not in the view)
            foreach (var ticket in Tickets)
            {
                // Sign the ticket payload (eventId + ticketId + uniqueCode + expiry)
                var signedToken = _signingService.SignTicket(
                    ticket.EventId,
                    ticket.Id,
                    ticket.UniqueCode,
                    ticket.Event.EventDate
                );

                // Generate QR code from the signed token
                TicketQRCodes[ticket.Id] = GenerateQRCode(signedToken);
            }

            return Page();
        }

        /// <summary>
        /// Generates a QR code from a signed ticket token.
        /// Called once per ticket in the page model, not in the view.
        /// The token contains HMAC signature to prevent forgery.
        /// </summary>
        private string GenerateQRCode(string signedToken)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(signedToken, QRCodeGenerator.ECCLevel.Q);
                using (BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData))
                {
                    byte[] qrCodeBytes = qrCode.GetGraphic(20);
                    return Convert.ToBase64String(qrCodeBytes);
                }
            }
        }
    }
}
