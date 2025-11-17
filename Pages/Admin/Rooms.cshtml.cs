using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CampusEvents.Models;
using CampusEvents.Services;

namespace CampusEvents.Pages.Admin;

public class RoomsModel : PageModel
{
    private readonly RoomRentalService _roomRentalService;

    public RoomsModel(RoomRentalService roomRentalService)
    {
        _roomRentalService = roomRentalService;
    }

    public List<Room> Rooms { get; set; } = new();
    public List<RoomRental> AllRentals { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
            return RedirectToPage("/Index");

        // Get all rooms
        Rooms = await _roomRentalService.GetAllRoomsAsync();

        // Get all rentals
        AllRentals = await _roomRentalService.GetAllRentalsAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostEnableAsync(int roomId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
            return RedirectToPage("/Index");

        var result = await _roomRentalService.EnableRoomAsync(roomId);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDisableAsync(int roomId, string reason)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
            return RedirectToPage("/Index");

        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Reason is required to disable a room.";
            return RedirectToPage();
        }

        var result = await _roomRentalService.DisableRoomAsync(roomId, reason);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelRentalAsync(int rentalId, string? reason)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
            return RedirectToPage("/Index");

        var result = await _roomRentalService.AdminCancelRentalAsync(rentalId, reason);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }
}
