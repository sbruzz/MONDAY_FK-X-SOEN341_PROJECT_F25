using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CampusEvents.Models;
using CampusEvents.Services;
using System.ComponentModel.DataAnnotations;

namespace CampusEvents.Pages.Student;

public class BecomeDriverModel : PageModel
{
    private readonly CarpoolService _carpoolService;

    public BecomeDriverModel(CarpoolService carpoolService)
    {
        _carpoolService = carpoolService;
    }

    [BindProperty]
    [Required]
    [Range(1, 50)]
    public int Capacity { get; set; }

    [BindProperty]
    [Required]
    public string VehicleType { get; set; } = string.Empty;

    [BindProperty]
    public string? LicensePlate { get; set; }

    public string? ErrorMessage { get; set; }
    public bool IsEdit { get; set; }

    public async Task<IActionResult> OnGetAsync(bool edit = false)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Student")
            return RedirectToPage("/Index");

        IsEdit = edit;

        // If editing, load existing driver profile
        if (edit)
        {
            var drivers = await _carpoolService.GetAllDriversAsync();
            var driver = drivers.FirstOrDefault(d => d.UserId == userId.Value);

            if (driver == null)
                return RedirectToPage("/Student/BecomeDriver");

            Capacity = driver.Capacity;
            VehicleType = driver.VehicleType.ToString();
            LicensePlate = driver.LicensePlate;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(List<string>? AccessibilityFeatures)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please fix the validation errors.";
            return Page();
        }

        // Parse vehicle type
        if (!Enum.TryParse<VehicleType>(VehicleType, out var vehicleType))
        {
            ErrorMessage = "Invalid vehicle type selected.";
            return Page();
        }

        // Combine accessibility features
        var accessibilityString = AccessibilityFeatures != null
            ? string.Join(",", AccessibilityFeatures)
            : string.Empty;

        // Register as driver
        var result = await _carpoolService.RegisterDriverAsync(
            userId.Value,
            Capacity,
            vehicleType,
            DriverType.Student,
            LicensePlate,
            accessibilityString);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Student/Carpools");
        }

        ErrorMessage = result.Message;
        return Page();
    }
}
