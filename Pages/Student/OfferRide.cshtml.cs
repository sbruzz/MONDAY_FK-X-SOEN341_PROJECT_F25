using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CampusEvents.Models;
using CampusEvents.Services;
using CampusEvents.Data;
using System.ComponentModel.DataAnnotations;

namespace CampusEvents.Pages.Student;

public class OfferRideModel : PageModel
{
    private readonly CarpoolService _carpoolService;
    private readonly AppDbContext _context;

    public OfferRideModel(CarpoolService carpoolService, AppDbContext context)
    {
        _carpoolService = carpoolService;
        _context = context;
    }

    [BindProperty]
    public int EventId { get; set; }

    [BindProperty]
    [Required]
    public string DepartureInfo { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string DepartureTime { get; set; } = string.Empty;

    [BindProperty]
    public string? DepartureAddress { get; set; }

    public Event? Event { get; set; }
    public Driver? DriverProfile { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Student")
            return RedirectToPage("/Index");

        EventId = eventId;

        // Get event
        Event = await _context.Events.FindAsync(eventId);
        if (Event == null)
            return NotFound();

        // Get user's driver profile
        var drivers = await _carpoolService.GetAllDriversAsync();
        DriverProfile = drivers.FirstOrDefault(d => d.UserId == userId.Value);

        if (DriverProfile == null)
        {
            TempData["ErrorMessage"] = "You must be a registered driver to offer rides.";
            return RedirectToPage("/Student/BecomeDriver");
        }

        if (DriverProfile.Status != DriverStatus.Active)
        {
            TempData["ErrorMessage"] = $"Your driver status is {DriverProfile.Status}. Only active drivers can offer rides.";
            return RedirectToPage("/Student/Carpools");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        // Reload event and driver profile for display
        Event = await _context.Events.FindAsync(EventId);
        var drivers = await _carpoolService.GetAllDriversAsync();
        DriverProfile = drivers.FirstOrDefault(d => d.UserId == userId.Value);

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please fix the validation errors.";
            return Page();
        }

        if (DriverProfile == null)
        {
            TempData["ErrorMessage"] = "Driver profile not found.";
            return RedirectToPage("/Student/BecomeDriver");
        }

        // Parse departure time and combine with event date
        if (!TimeSpan.TryParse(DepartureTime, out var departureTimeSpan))
        {
            ErrorMessage = "Invalid departure time format.";
            return Page();
        }

        var departureDateTime = Event!.EventDate.Date.Add(departureTimeSpan);

        // Create carpool offer
        var result = await _carpoolService.CreateOfferAsync(
            DriverProfile.Id,
            EventId,
            DepartureInfo,
            departureDateTime,
            DepartureAddress);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Student/Carpools");
        }

        ErrorMessage = result.Message;
        return Page();
    }
}
