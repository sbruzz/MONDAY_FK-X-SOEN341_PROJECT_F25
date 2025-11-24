using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using System.ComponentModel.DataAnnotations;

namespace CampusEvents.Pages.Organizer;

public class DriversModel : PageModel
{
    private readonly AppDbContext _context;

    public DriversModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Driver> Drivers { get; set; } = new();
    public List<Event> Events { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public bool IsSuccess { get; set; }

    public class InputModel
    {
        public int? EventId { get; set; }

        [Required(ErrorMessage = "Vehicle type is required")]
        public VehicleType VehicleType { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 15, ErrorMessage = "Capacity must be between 1 and 15")]
        public int Capacity { get; set; }

        public bool HasAccessibility { get; set; }
        
        [StringLength(500)]
        public string? VehicleDescription { get; set; }

        [StringLength(20)]
        public string? LicensePlate { get; set; }

        [StringLength(20)]
        public string? ContactPhone { get; set; }

        [StringLength(100)]
        public string? DriverName { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null || user.Role != UserRole.Organizer)
        {
            return RedirectToPage("/Index");
        }

        // Load organizer's drivers
        Drivers = await _context.Drivers
            .Include(d => d.User)
            .Include(d => d.CarpoolOffers)
                .ThenInclude(o => o.Event)
            .Include(d => d.CarpoolOffers)
                .ThenInclude(o => o.Passengers)
                    .ThenInclude(p => p.Passenger)
            .Where(d => d.UserId == userId.Value)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        // Load organizer's events for dropdown
        Events = await _context.Events
            .Where(e => e.OrganizerId == userId.Value && e.ApprovalStatus == ApprovalStatus.Approved)
            .OrderByDescending(e => e.EventDate)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null || user.Role != UserRole.Organizer)
        {
            return RedirectToPage("/Index");
        }

        if (!ModelState.IsValid)
        {
            // Reload data
            Drivers = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.CarpoolOffers)
                    .ThenInclude(o => o.Event)
                .Where(d => d.UserId == userId.Value)
                .ToListAsync();

            Events = await _context.Events
                .Where(e => e.OrganizerId == userId.Value && e.ApprovalStatus == ApprovalStatus.Approved)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            return Page();
        }

        // Organizers can add multiple drivers (different vehicles/people)
        // Create new driver profile
        var driver = new Driver
        {
            UserId = userId.Value,
            DriverType = DriverType.Organizer,
            VehicleType = Input.VehicleType,
            Capacity = Input.Capacity,
            AccessibilityFeatures = Input.HasAccessibility ? "Wheelchair accessible" : "",
            LicensePlate = Input.LicensePlate ?? "",
            Status = DriverStatus.Active,
            SecurityFlags = "",
            History = "",
            CreatedAt = DateTime.UtcNow
        };

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        Message = "Driver added successfully!";

        // If an event was selected, create a carpool offer for it
        if (Input.EventId.HasValue)
        {
            // Check if offer already exists for this event
            var existingOffer = driver.CarpoolOffers.FirstOrDefault(o => o.EventId == Input.EventId.Value);

            if (existingOffer == null)
            {
                var eventData = await _context.Events.FindAsync(Input.EventId.Value);
                var carpoolOffer = new CarpoolOffer
                {
                    EventId = Input.EventId.Value,
                    DriverId = driver.Id,
                    SeatsAvailable = driver.Capacity,
                    DepartureInfo = $"Driving to {eventData?.Location ?? "event"}",
                    DepartureTime = eventData?.EventDate.AddHours(-1) ?? DateTime.UtcNow,
                    Status = CarpoolOfferStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CarpoolOffers.Add(carpoolOffer);
                await _context.SaveChangesAsync();

                Message += " Carpool offer created for the selected event.";
            }
        }

        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveDriverAsync(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        var driver = await _context.Drivers
            .Include(d => d.CarpoolOffers)
                .ThenInclude(o => o.Passengers)
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId.Value);

        if (driver == null)
        {
            Message = "Driver not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Remove carpool offers and their passengers first
        if (driver.CarpoolOffers.Any())
        {
            foreach (var offer in driver.CarpoolOffers)
            {
                if (offer.Passengers.Any())
                {
                    _context.CarpoolPassengers.RemoveRange(offer.Passengers);
                }
            }
            _context.CarpoolOffers.RemoveRange(driver.CarpoolOffers);
        }

        _context.Drivers.Remove(driver);
        await _context.SaveChangesAsync();

        Message = "Driver removed successfully.";
        IsSuccess = true;
        return RedirectToPage();
    }
}

