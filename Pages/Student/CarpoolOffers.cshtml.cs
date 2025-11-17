using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CampusEvents.Models;
using CampusEvents.Services;
using CampusEvents.Data;

namespace CampusEvents.Pages.Student;

public class CarpoolOffersModel : PageModel
{
    private readonly CarpoolService _carpoolService;
    private readonly AppDbContext _context;

    public CarpoolOffersModel(CarpoolService carpoolService, AppDbContext context)
    {
        _carpoolService = carpoolService;
        _context = context;
    }

    public int EventId { get; set; }
    public Event? Event { get; set; }
    public List<CarpoolOffer> Offers { get; set; } = new();
    public bool CanOfferRide { get; set; }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Student")
            return RedirectToPage("/Index");

        EventId = eventId;

        // Get event details
        Event = await _context.Events.FindAsync(eventId);
        if (Event == null)
            return NotFound();

        // Get all active offers for this event
        Offers = await _carpoolService.GetEventOffersAsync(eventId);

        // Check if user can offer a ride (is an active driver)
        var drivers = await _carpoolService.GetAllDriversAsync(DriverStatus.Active);
        var userDriver = drivers.FirstOrDefault(d => d.UserId == userId.Value);
        CanOfferRide = userDriver != null;

        return Page();
    }

    public async Task<IActionResult> OnPostJoinRideAsync(int offerId, string? pickupLocation, string? notes)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var result = await _carpoolService.JoinOfferAsync(offerId, userId.Value, pickupLocation, notes);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Student/Carpools");
        }

        TempData["ErrorMessage"] = result.Message;
        return RedirectToPage();
    }
}
