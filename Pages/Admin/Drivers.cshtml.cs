using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CampusEvents.Models;
using CampusEvents.Services;

namespace CampusEvents.Pages.Admin;

public class DriversModel : PageModel
{
    private readonly CarpoolService _carpoolService;

    public DriversModel(CarpoolService carpoolService)
    {
        _carpoolService = carpoolService;
    }

    public List<Driver> Drivers { get; set; } = new();
    public List<CarpoolPassenger> AllPassengers { get; set; } = new();
    public string? StatusFilter { get; set; }

    public async Task<IActionResult> OnGetAsync(string? status)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
            return RedirectToPage("/Index");

        StatusFilter = status;

        // Get drivers based on filter
        if (!string.IsNullOrEmpty(status) && status != "all")
        {
            if (Enum.TryParse<DriverStatus>(status, out var driverStatus))
            {
                Drivers = await _carpoolService.GetAllDriversAsync(driverStatus);
            }
            else
            {
                Drivers = await _carpoolService.GetAllDriversAsync();
            }
        }
        else
        {
            Drivers = await _carpoolService.GetAllDriversAsync();
        }

        // Get all passengers for management
        AllPassengers = await _carpoolService.GetAllPassengersAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int driverId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
            return RedirectToPage("/Index");

        var result = await _carpoolService.ApproveDriverAsync(driverId);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSuspendAsync(int driverId, string reason)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
            return RedirectToPage("/Index");

        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Suspension reason is required.";
            return RedirectToPage();
        }

        var result = await _carpoolService.SuspendDriverAsync(driverId, reason);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnsuspendAsync(int driverId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
            return RedirectToPage("/Index");

        var result = await _carpoolService.UnsuspendDriverAsync(driverId);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReassignPassengerAsync(int passengerId, int newOfferId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
            return RedirectToPage("/Index");

        var result = await _carpoolService.ReassignPassengerAsync(passengerId, newOfferId);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }
}
