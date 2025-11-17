using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CampusEvents.Models;
using CampusEvents.Services;

namespace CampusEvents.Pages.Student;

public class RoomsModel : PageModel
{
    private readonly RoomRentalService _roomRentalService;

    public RoomsModel(RoomRentalService roomRentalService)
    {
        _roomRentalService = roomRentalService;
    }

    public List<Room> AvailableRooms { get; set; } = new();
    public List<RoomRental> MyRentals { get; set; } = new();
    public DateTime? StartTimeFilter { get; set; }
    public DateTime? EndTimeFilter { get; set; }
    public int? MinCapacityFilter { get; set; }

    public async Task<IActionResult> OnGetAsync(DateTime? startTime, DateTime? endTime, int? minCapacity)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Student")
            return RedirectToPage("/Index");

        // Store filter values
        StartTimeFilter = startTime;
        EndTimeFilter = endTime;
        MinCapacityFilter = minCapacity;

        // Get user's rental history
        MyRentals = await _roomRentalService.GetUserRentalsAsync(userId.Value);

        // Get available rooms based on filters
        if (startTime.HasValue && endTime.HasValue)
        {
            AvailableRooms = await _roomRentalService.GetAvailableRoomsAsync(
                startTime.Value,
                endTime.Value,
                minCapacity);
        }
        else
        {
            // If no time filter, just show all enabled rooms
            AvailableRooms = await _roomRentalService.GetRoomsAsync(
                onlyEnabled: true,
                minCapacity: minCapacity);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRequestRentalAsync(
        int roomId,
        DateTime startTime,
        DateTime endTime,
        string? purpose,
        int? expectedAttendees)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var result = await _roomRentalService.RequestRentalAsync(
            roomId,
            userId.Value,
            startTime,
            endTime,
            purpose,
            expectedAttendees);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelRentalAsync(int rentalId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var result = await _roomRentalService.CancelRentalAsync(rentalId, userId.Value);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToPage();
    }
}
