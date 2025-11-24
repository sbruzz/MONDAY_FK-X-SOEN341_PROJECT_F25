using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Services;

namespace CampusEvents.Pages.Organizer
{
    public class QRScannerModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly TicketSigningService _signingService;

        // NOTE: In-memory rate limiting won't work across multiple app instances.
        // For production multi-instance deployments, move to a shared store (Redis, database, etc.)
        private static readonly Dictionary<string, DateTime> _scanAttempts = new();
        private static readonly TimeSpan RATE_LIMIT_WINDOW = TimeSpan.FromSeconds(2);
        private const int MAX_ATTEMPTS_PER_WINDOW = 1;

        public QRScannerModel(AppDbContext context, TicketSigningService signingService)
        {
            _context = context;
            _signingService = signingService;
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

        public async Task<IActionResult> OnPostValidateCodeAsync(int eventId, string ticketCode)
        {
            Console.WriteLine("========== QR VALIDATION STARTED ==========");
            Console.WriteLine($"Event ID: {eventId}");
            Console.WriteLine($"Ticket Code Length: {ticketCode?.Length ?? 0}");

            var userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine($"Organizer User ID: {userId}");

            if (userId == null)
            {
                Console.WriteLine("ERROR: User not logged in");
                return RedirectToPage("/Login");
            }

            // Server-side rate limiting to prevent abuse/replay attacks
            // Track by both user+code AND IP address for defense in depth
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userCodeKey = $"user:{userId}:{ticketCode}";
            var ipKey = $"ip:{clientIp}";
            var now = DateTime.UtcNow;
            bool rateLimitExceeded = false;

            lock (_scanAttempts)
            {
                // Clean up old entries
                var expiredKeys = _scanAttempts
                    .Where(kvp => now - kvp.Value > RATE_LIMIT_WINDOW)
                    .Select(kvp => kvp.Key)
                    .ToList();
                foreach (var key in expiredKeys)
                {
                    _scanAttempts.Remove(key);
                }

                // Check rate limit for user+code combination
                if (_scanAttempts.TryGetValue(userCodeKey, out var lastUserAttempt))
                {
                    if (now - lastUserAttempt < RATE_LIMIT_WINDOW)
                    {
                        rateLimitExceeded = true;
                    }
                }

                // Also check rate limit by IP address (prevents rapid scanning from same device)
                if (_scanAttempts.TryGetValue(ipKey, out var lastIpAttempt))
                {
                    if (now - lastIpAttempt < RATE_LIMIT_WINDOW)
                    {
                        rateLimitExceeded = true;
                    }
                }

                if (!rateLimitExceeded)
                {
                    _scanAttempts[userCodeKey] = now;
                    _scanAttempts[ipKey] = now;
                }
            }

            if (rateLimitExceeded)
            {
                Console.WriteLine("WARNING: Rate limit exceeded for this scan attempt");
                ResultMessage = "Rate limit exceeded. Please wait a moment before scanning again.";
                IsSuccess = false;
                await LoadEventStats(eventId);
                return Page();
            }

            Console.WriteLine("Rate limit check passed");

            // Load event
            Event = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (Event == null || Event.OrganizerId != userId.Value)
            {
                return RedirectToPage("/Organizer/Events");
            }

            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                Console.WriteLine("ERROR: Ticket code is empty or whitespace");
                ResultMessage = "Please enter a ticket code.";
                IsSuccess = false;
                await LoadEventStats(eventId);
                return Page();
            }

            Console.WriteLine($"Ticket code received (first 50 chars): {ticketCode.Substring(0, Math.Min(50, ticketCode.Length))}...");

            // Verify HMAC signature and token validity
            Console.WriteLine("Verifying HMAC signature...");
            var validationResult = _signingService.VerifyTicket(ticketCode.Trim());
            if (validationResult == null || !validationResult.IsValid)
            {
                Console.WriteLine($"ERROR: Signature verification failed: {validationResult?.ErrorMessage ?? "Verification failed"}");
                ResultMessage = $"Invalid or tampered ticket: {validationResult?.ErrorMessage ?? "Verification failed"}";
                IsSuccess = false;
                await LoadEventStats(eventId);
                return Page();
            }

            Console.WriteLine("✓ Signature verified successfully");
            var payload = validationResult.Payload!;
            Console.WriteLine($"Payload - TicketId: {payload.TicketId}, EventId: {payload.EventId}, UniqueCode: {payload.UniqueCode?.Substring(0, Math.Min(20, payload.UniqueCode?.Length ?? 0))}...");

            // Verify ticket is for this event
            if (payload.EventId != eventId)
            {
                Console.WriteLine($"ERROR: Event ID mismatch. Payload EventId: {payload.EventId}, Expected: {eventId}");
                ResultMessage = "This ticket is for a different event.";
                IsSuccess = false;
                await LoadEventStats(eventId);
                return Page();
            }

            Console.WriteLine($"✓ Event ID matches: {eventId}");

            // Find ticket by ID and UniqueCode (double verification prevents replay after signature passes)
            Console.WriteLine($"Querying database for ticket ID {payload.TicketId}...");
            ValidatedTicket = await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == payload.TicketId && t.UniqueCode == payload.UniqueCode && t.EventId == eventId);

            if (ValidatedTicket == null)
            {
                Console.WriteLine($"ERROR: Ticket not found in database. TicketId: {payload.TicketId}, EventId: {eventId}");
                ResultMessage = "Ticket not found or data mismatch. This may indicate a forged or altered ticket.";
                IsSuccess = false;
                await LoadEventStats(eventId);
                return Page();
            }

            Console.WriteLine($"✓ Ticket found in database - ID: {ValidatedTicket.Id}, User: {ValidatedTicket.User.Name}, IsRedeemed: {ValidatedTicket.IsRedeemed}");

            // Check if already redeemed
            if (ValidatedTicket.IsRedeemed)
            {
                Console.WriteLine($"WARNING: Ticket already redeemed at {ValidatedTicket.RedeemedAt}");
                ResultMessage = $"Ticket Already Used! This ticket was checked in on {ValidatedTicket.RedeemedAt?.ToString("MMM dd, yyyy h:mm tt")}.";
                IsSuccess = false;
                await LoadEventStats(eventId);
                return Page();
            }

            Console.WriteLine("Ticket is not yet redeemed - proceeding to mark as checked in");
            Console.WriteLine($"Current values BEFORE update - IsRedeemed: {ValidatedTicket.IsRedeemed}, RedeemedAt: {ValidatedTicket.RedeemedAt}");

            // Mark ticket as redeemed
            var redemptionTime = DateTime.UtcNow;
            ValidatedTicket.IsRedeemed = true;
            ValidatedTicket.RedeemedAt = redemptionTime;

            Console.WriteLine($"Values AFTER setting - IsRedeemed: {ValidatedTicket.IsRedeemed}, RedeemedAt: {ValidatedTicket.RedeemedAt}");

            // Ensure the ticket is being tracked and saved
            Console.WriteLine($"Entity state BEFORE setting to Modified: {_context.Entry(ValidatedTicket).State}");
            _context.Entry(ValidatedTicket).State = EntityState.Modified;
            Console.WriteLine($"Entity state AFTER setting to Modified: {_context.Entry(ValidatedTicket).State}");

            Console.WriteLine("Calling SaveChangesAsync()...");
            try
            {
                var saveResult = await _context.SaveChangesAsync();
                Console.WriteLine($"✓ SaveChangesAsync() completed! Rows affected: {saveResult}");

                if (saveResult == 0)
                {
                    Console.WriteLine("WARNING: SaveChangesAsync returned 0 - no rows were updated in the database!");
                }

                // Verify the save by re-querying
                var verifyTicket = await _context.Tickets.FindAsync(ValidatedTicket.Id);
                Console.WriteLine($"Verification query - IsRedeemed: {verifyTicket?.IsRedeemed}, RedeemedAt: {verifyTicket?.RedeemedAt}");

                ResultMessage = $"✓ Ticket Valid! Attendee Successfully Checked In. (DB Updated: {saveResult > 0})";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR during SaveChangesAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                throw;
            }

            Console.WriteLine("========== QR VALIDATION COMPLETED ==========");
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
