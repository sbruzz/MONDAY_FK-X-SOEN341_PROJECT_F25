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
            .Include(d => d.Event)
            .Include(d => d.Passengers)
            .Where(d => d.OrganizerId == userId.Value)
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
                .Include(d => d.Event)
                .Where(d => d.OrganizerId == userId.Value)
                .ToListAsync();

            Events = await _context.Events
                .Where(e => e.OrganizerId == userId.Value && e.ApprovalStatus == ApprovalStatus.Approved)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            return Page();
        }

        // Create a driver entry
        // Note: For organizer-created drivers, we might need a separate User entry or use organizer's user
        // For now, we'll create a driver entry linked to the organizer
        var driver = new Driver
        {
            UserId = userId.Value, // Organizer's user ID
            OrganizerId = userId.Value,
            EventId = Input.EventId,
            Type = DriverType.Organizer,
            VehicleType = Input.VehicleType,
            Capacity = Input.Capacity,
            HasAccessibility = Input.HasAccessibility,
            VehicleDescription = Input.VehicleDescription,
            LicensePlate = Input.LicensePlate,
            ContactPhone = Input.ContactPhone ?? "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        Message = "Driver created successfully!";
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
            .Include(d => d.Passengers)
            .FirstOrDefaultAsync(d => d.Id == id && d.OrganizerId == userId.Value);

        if (driver == null)
        {
            Message = "Driver not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Remove passengers first
        if (driver.Passengers.Any())
        {
            _context.Passengers.RemoveRange(driver.Passengers);
        }

        _context.Drivers.Remove(driver);
        await _context.SaveChangesAsync();

        Message = "Driver removed successfully.";
        IsSuccess = true;
        return RedirectToPage();
    }
}

