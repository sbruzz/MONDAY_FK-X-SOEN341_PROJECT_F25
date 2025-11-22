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
        private readonly ILogger<EventDetailsModel> _logger;

        public EventDetailsModel(AppDbContext context, ILogger<EventDetailsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Event? Event { get; set; }
        public bool HasTicket { get; set; }
        public bool IsSaved { get; set; }
        public List<Driver> AvailableDrivers { get; set; } = new();
        public bool IsDriver { get; set; }
        public bool IsPassenger { get; set; }
        public bool ShouldShowDriverPrompt { get; set; }

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public bool IsSuccess { get; set; }

        [BindProperty]
        public DriverInputModel DriverInput { get; set; } = new();

        public class DriverInputModel
        {
            public VehicleType VehicleType { get; set; }
            public int Capacity { get; set; }
            public bool HasAccessibility { get; set; }
            public string? VehicleDescription { get; set; }
            public string? LicensePlate { get; set; }
            public string? ContactPhone { get; set; }
        }

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

            // Check carpool status
            IsDriver = await _context.Drivers
                .AnyAsync(d => d.UserId == userId.Value && d.EventId == id && d.IsActive);
            
            IsPassenger = await _context.CarpoolPassengers
                .AnyAsync(p => p.UserId == userId.Value && p.EventId == id);

            // Load available drivers for this event
            AvailableDrivers = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Passengers)
                .Where(d => d.EventId == id && d.IsActive && !d.IsMarkedByAdmin)
                .ToListAsync();

            // Show driver prompt if: has ticket, not already driver/passenger, event has location
            ShouldShowDriverPrompt = HasTicket && !IsDriver && !IsPassenger && !string.IsNullOrEmpty(Event.Location);

            return Page();
        }

        public async Task<IActionResult> OnPostClaimTicketAsync(int id)
        {
            _logger.LogInformation("========== CLAIM TICKET STARTED ==========");
            _logger.LogInformation("Event ID parameter received: {EventId}", id);
            Console.WriteLine($"========== CLAIM TICKET STARTED ==========");
            Console.WriteLine($"Event ID parameter received: {id}");

            var userId = HttpContext.Session.GetInt32("UserId");
            _logger.LogInformation("User ID from session: {UserId}", userId);
            Console.WriteLine($"User ID from session: {userId}");

            if (userId == null)
            {
                _logger.LogWarning("User not logged in, redirecting to login");
                Console.WriteLine("User not logged in, redirecting to login");
                return RedirectToPage("/Login");
            }

            // Reload event
            _logger.LogInformation("Loading event with ID: {EventId}", id);
            Console.WriteLine($"Loading event with ID: {id}");

            Event = await _context.Events
                .Include(e => e.Organization)
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (Event == null)
            {
                _logger.LogError("Event not found with ID: {EventId}", id);
                Console.WriteLine($"ERROR: Event not found with ID: {id}");
                Message = "Event not found.";
                IsSuccess = false;
                return RedirectToPage(new { id });
            }

            _logger.LogInformation("Event found: {EventTitle}, Capacity: {Capacity}, Tickets Issued: {TicketsIssued}, Price: {Price}",
                Event.Title, Event.Capacity, Event.TicketsIssued, Event.Price);
            Console.WriteLine($"Event found: {Event.Title}, Capacity: {Event.Capacity}, Tickets Issued: {Event.TicketsIssued}, Price: {Event.Price}");

            // Check if user already has a ticket
            _logger.LogInformation("Checking if user {UserId} already has ticket for event {EventId}", userId.Value, id);
            Console.WriteLine($"Checking if user {userId.Value} already has ticket for event {id}");

            var existingTicket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.EventId == id && t.UserId == userId.Value);

            if (existingTicket != null)
            {
                _logger.LogWarning("User {UserId} already has ticket for event {EventId}", userId.Value, id);
                Console.WriteLine($"WARNING: User {userId.Value} already has ticket for event {id}");
                Message = "You already have a ticket for this event!";
                IsSuccess = false;
                return RedirectToPage(new { id });
            }

            _logger.LogInformation("User does not have existing ticket");
            Console.WriteLine("User does not have existing ticket");

            // Check capacity
            _logger.LogInformation("Checking capacity: {TicketsIssued}/{Capacity}", Event.TicketsIssued, Event.Capacity);
            Console.WriteLine($"Checking capacity: {Event.TicketsIssued}/{Event.Capacity}");

            if (Event.TicketsIssued >= Event.Capacity)
            {
                _logger.LogWarning("Event {EventId} is full", id);
                Console.WriteLine($"WARNING: Event {id} is full");
                Message = "Sorry, this event is full.";
                IsSuccess = false;
                return RedirectToPage(new { id });
            }

            // Generate unique code for ticket (canonical source of truth)
            var uniqueCode = Guid.NewGuid().ToString();
            _logger.LogInformation("Generated unique ticket code: {UniqueCode}", uniqueCode);

            // Create ticket (QR code will be generated on-demand from UniqueCode)
            var ticket = new Ticket
            {
                EventId = id,
                UserId = userId.Value,
                UniqueCode = uniqueCode,
                ClaimedAt = DateTime.UtcNow,
                IsRedeemed = false
            };

            _logger.LogInformation("Adding ticket to database context...");
            Console.WriteLine("Adding ticket to database context...");
            _context.Tickets.Add(ticket);

            // Update event tickets issued count
            Event.TicketsIssued++;
            _logger.LogInformation("Incremented tickets issued count to: {TicketsIssued}", Event.TicketsIssued);
            Console.WriteLine($"Incremented tickets issued count to: {Event.TicketsIssued}");

            try
            {
                _logger.LogInformation("Saving changes to database...");
                Console.WriteLine("Saving changes to database...");
                await _context.SaveChangesAsync();
                _logger.LogInformation("Database save successful!");
                Console.WriteLine("Database save successful!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR saving to database: {Message}", ex.Message);
                Console.WriteLine($"ERROR saving to database: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }

            Message = Event.Price == 0
                ? "Ticket claimed successfully!"
                : $"Ticket purchased successfully! (Mock payment of ${Event.Price:F2} processed)";
            IsSuccess = true;

            _logger.LogInformation("Ticket claim SUCCESS! Message: {Message}", Message);
            Console.WriteLine($"Ticket claim SUCCESS! Message: {Message}");
            _logger.LogInformation("========== CLAIM TICKET COMPLETED ==========");
            Console.WriteLine("========== CLAIM TICKET COMPLETED ==========");

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

        public async Task<IActionResult> OnPostBecomeDriverAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                return await OnGetAsync(id);
            }

            // Check if already a driver
            var existingDriver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == userId.Value && d.EventId == id);
            
            if (existingDriver != null)
            {
                Message = "You are already registered as a driver for this event.";
                IsSuccess = false;
                return RedirectToPage(new { id });
            }

            var driver = new Driver
            {
                UserId = userId.Value,
                EventId = id,
                Type = DriverType.Student,
                VehicleType = DriverInput.VehicleType,
                Capacity = DriverInput.Capacity,
                HasAccessibility = DriverInput.HasAccessibility,
                VehicleDescription = DriverInput.VehicleDescription,
                LicensePlate = DriverInput.LicensePlate,
                ContactPhone = DriverInput.ContactPhone ?? "",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();

            Message = "You are now registered as a driver! Students can request to ride with you.";
            IsSuccess = true;
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostChooseDriverAsync(int id, int driverId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            // Check if already a passenger
            var existingPassenger = await _context.CarpoolPassengers
                .FirstOrDefaultAsync(p => p.UserId == userId.Value && p.EventId == id);
            
            if (existingPassenger != null)
            {
                Message = "You are already assigned to a driver for this event.";
                IsSuccess = false;
                return RedirectToPage(new { id });
            }

            // Check driver capacity
            var driver = await _context.Drivers
                .Include(d => d.Passengers)
                .FirstOrDefaultAsync(d => d.Id == driverId && d.EventId == id);
            
            if (driver == null)
            {
                Message = "Driver not found.";
                IsSuccess = false;
                return RedirectToPage(new { id });
            }

            if (driver.Passengers.Count >= driver.Capacity)
            {
                Message = "This driver has reached their capacity.";
                IsSuccess = false;
                return RedirectToPage(new { id });
            }

            var passenger = new CarpoolPassenger
            {
                DriverId = driverId,
                UserId = userId.Value,
                EventId = id,
                AssignedAt = DateTime.UtcNow
            };

            _context.CarpoolPassengers.Add(passenger);
            await _context.SaveChangesAsync();

            Message = $"You are now riding with {driver.User.Name}!";
            IsSuccess = true;
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeclineCarpoolAsync(int id)
        {
            // Just set a flag or do nothing - user declined
            Message = "You can always set up carpooling later if you change your mind.";
            IsSuccess = true;
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
