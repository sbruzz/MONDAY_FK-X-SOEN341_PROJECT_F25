using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using System.ComponentModel.DataAnnotations;

namespace CampusEvents.Pages.Student;

public class RentalsModel : PageModel
{
    private readonly AppDbContext _context;

    public RentalsModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Room> AvailableRooms { get; set; } = new();
    public List<RoomRental> MyRentals { get; set; } = new();

    [BindProperty]
    public RentalInputModel RentalInput { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public bool IsSuccess { get; set; }

    public class RentalInputModel
    {
        [Required(ErrorMessage = "Room is required")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        [StringLength(500)]
        public string? Purpose { get; set; }

        public int? EventId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        // Load available rooms (enabled only)
        AvailableRooms = await _context.Rooms
            .Where(r => r.Status == RoomStatus.Enabled)
            .OrderBy(r => r.Name)
            .ToListAsync();

        // Load user's rentals
        MyRentals = await _context.RoomRentals
            .Include(r => r.Room)
            .Include(r => r.Renter)
            .Where(r => r.RenterId == userId.Value)
            .OrderByDescending(r => r.StartTime)
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

        if (!ModelState.IsValid)
        {
            AvailableRooms = await _context.Rooms
                .Where(r => r.Status == RoomStatus.Enabled)
                .ToListAsync();
            MyRentals = await _context.RoomRentals
                .Include(r => r.Room)
                .Where(r => r.RenterId == userId.Value)
                .ToListAsync();
            return Page();
        }

        // Validate time range
        if (RentalInput.EndTime <= RentalInput.StartTime)
        {
            ModelState.AddModelError("RentalInput.EndTime", "End time must be after start time.");
            AvailableRooms = await _context.Rooms.Where(r => r.Status == RoomStatus.Enabled).ToListAsync();
            MyRentals = await _context.RoomRentals
                .Include(r => r.Room)
                .Where(r => r.RenterId == userId.Value)
                .ToListAsync();
            return Page();
        }

        // Check if room is available for the time slot
        // Check for conflicting rentals (Rented or Disabled status)
        var conflictingRentals = await _context.RoomRentals
            .Where(r => r.RoomId == RentalInput.RoomId 
                && (r.Status == RentalStatus.Approved || r.Status == RentalStatus.Rejected)
                && ((r.StartTime <= RentalInput.StartTime && r.EndTime > RentalInput.StartTime) ||
                    (r.StartTime < RentalInput.EndTime && r.EndTime >= RentalInput.EndTime) ||
                    (r.StartTime >= RentalInput.StartTime && r.EndTime <= RentalInput.EndTime)))
            .ToListAsync();

        if (conflictingRentals.Any())
        {
            Message = "This room is not available for the selected time slot.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Check if room is disabled
        var room = await _context.Rooms.FindAsync(RentalInput.RoomId);
        if (room == null || !room.IsEnabled)
        {
            Message = "This room is not available for rental.";
            IsSuccess = false;
            return RedirectToPage();
        }

        var rental = new RoomRental
        {
            RoomId = RentalInput.RoomId,
            RenterId = userId.Value,
            StartTime = RentalInput.StartTime,
            EndTime = RentalInput.EndTime,
            Purpose = RentalInput.Purpose ?? "",
            Status = RentalStatus.Approved,
            AdminNotes = "",
            ExpectedAttendees = 0,
            TotalCost = 0m,
            CreatedAt = DateTime.UtcNow
        };

        _context.RoomRentals.Add(rental);
        await _context.SaveChangesAsync();

        Message = "Room rental request submitted successfully!";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelRentalAsync(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        var rental = await _context.RoomRentals
            .FirstOrDefaultAsync(r => r.Id == id && r.RenterId == userId.Value);

        if (rental == null)
        {
            Message = "Rental not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Only allow cancellation if rental hasn't started
        if (rental.StartTime > DateTime.UtcNow)
        {
            _context.RoomRentals.Remove(rental);
            await _context.SaveChangesAsync();
            Message = "Rental cancelled successfully.";
            IsSuccess = true;
        }
        else
        {
            Message = "Cannot cancel a rental that has already started.";
            IsSuccess = false;
        }

        return RedirectToPage();
    }
}

