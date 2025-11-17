using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CampusEvents.Models;
using CampusEvents.Services;

namespace CampusEvents.Pages.Organizer;

public class RoomsModel : PageModel
{
    private readonly RoomRentalService _roomRentalService;

    public RoomsModel(RoomRentalService roomRentalService)
    {
        _roomRentalService = roomRentalService;
    }

    public List<Room> MyRooms { get; set; } = new();
    public List<RoomRental> PendingRentals { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Organizer")
            return RedirectToPage("/Index");

        // Get organizer's rooms
        MyRooms = await _roomRentalService.GetOrganizerRoomsAsync(userId.Value);

        // Get pending rental requests for organizer's rooms
        PendingRentals = await _roomRentalService.GetPendingRentalsForOrganizerAsync(userId.Value);

        return Page();
    }

    public async Task<IActionResult> OnPostCreateRoomAsync(
        string name,
        string address,
        int capacity,
        string? roomInfo,
        string? amenities,
        decimal? hourlyRate,
        DateTime? availabilityStart,
        DateTime? availabilityEnd)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Organizer")
            return RedirectToPage("/Index");

        var result = await _roomRentalService.CreateRoomAsync(
            userId.Value,
            name,
            address,
            capacity,
            roomInfo,
            amenities ?? string.Empty,
            hourlyRate,
            availabilityStart,
            availabilityEnd);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostApproveRentalAsync(int rentalId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Organizer")
            return RedirectToPage("/Index");

        var result = await _roomRentalService.ApproveRentalAsync(rentalId, userId.Value, isAdmin: false);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectRentalAsync(int rentalId, string? reason)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Organizer")
            return RedirectToPage("/Index");

        var result = await _roomRentalService.RejectRentalAsync(rentalId, userId.Value, reason, isAdmin: false);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }
}
