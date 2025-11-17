using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using System.ComponentModel.DataAnnotations;

namespace CampusEvents.Pages.Admin;

public class RentalsModel : PageModel
{
    private readonly AppDbContext _context;

    public RentalsModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Room> Rooms { get; set; } = new();
    public List<Rental> AllRentals { get; set; } = new();

    [BindProperty]
    public RoomInputModel RoomInput { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public bool IsSuccess { get; set; }

    public class RoomInputModel
    {
        [Required(ErrorMessage = "Room name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        [StringLength(300)]
        public string Location { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 1000)]
        public int Capacity { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null || user.Role != UserRole.Admin)
        {
            return RedirectToPage("/Index");
        }

        Rooms = await _context.Rooms
            .Include(r => r.Rentals)
            .OrderBy(r => r.Name)
            .ToListAsync();

        AllRentals = await _context.Rentals
            .Include(r => r.Room)
            .Include(r => r.User)
            .Include(r => r.Event)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostCreateRoomAsync()
    {
        if (!ModelState.IsValid)
        {
            Rooms = await _context.Rooms.Include(r => r.Rentals).ToListAsync();
            AllRentals = await _context.Rentals
                .Include(r => r.Room)
                .Include(r => r.User)
                .ToListAsync();
            return Page();
        }

        var room = new Room
        {
            Name = RoomInput.Name,
            Location = RoomInput.Location,
            Description = RoomInput.Description,
            Capacity = RoomInput.Capacity,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        Message = "Room created successfully!";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleRoomAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            Message = "Room not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        room.IsEnabled = !room.IsEnabled;
        await _context.SaveChangesAsync();

        Message = $"Room {(room.IsEnabled ? "enabled" : "disabled")} successfully.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteRoomAsync(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.Rentals)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
        {
            Message = "Room not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        if (room.Rentals.Any())
        {
            Message = "Cannot delete room with active rentals. Disable it instead.";
            IsSuccess = false;
            return RedirectToPage();
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        Message = "Room deleted successfully.";
        IsSuccess = true;
        return RedirectToPage();
    }
}

