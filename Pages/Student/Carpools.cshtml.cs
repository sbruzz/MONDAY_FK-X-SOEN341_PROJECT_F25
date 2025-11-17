using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CampusEvents.Models;
using CampusEvents.Services;

namespace CampusEvents.Pages.Student;

public class CarpoolsModel : PageModel
{
    private readonly CarpoolService _carpoolService;

    public CarpoolsModel(CarpoolService carpoolService)
    {
        _carpoolService = carpoolService;
    }

    public Driver? DriverProfile { get; set; }
    public List<CarpoolOffer> OffersAsDriver { get; set; } = new();
    public List<CarpoolPassenger> RidesAsPassenger { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Student")
            return RedirectToPage("/Index");

        // Get user's driver profile if exists
        var allDrivers = await _carpoolService.GetAllDriversAsync();
        DriverProfile = allDrivers.FirstOrDefault(d => d.UserId == userId.Value);

        // Get user's carpools (as driver and passenger)
        var (asDriver, asPassenger) = await _carpoolService.GetUserCarpoolsAsync(userId.Value);
        OffersAsDriver = asDriver;
        RidesAsPassenger = asPassenger;

        return Page();
    }

    public async Task<IActionResult> OnPostCancelOfferAsync(int offerId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var result = await _carpoolService.CancelOfferAsync(offerId, userId.Value);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLeaveRideAsync(int offerId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var result = await _carpoolService.LeaveOfferAsync(offerId, userId.Value);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }
}
