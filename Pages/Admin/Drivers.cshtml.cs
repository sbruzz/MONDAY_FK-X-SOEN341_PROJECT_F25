using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages.Admin;

public class DriversModel : PageModel
{
    private readonly AppDbContext _context;

    public DriversModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Driver> Drivers { get; set; } = new();
    public List<Passenger> AllPassengers { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? TypeFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public bool IsSuccess { get; set; }

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

        // Load drivers with related data
        var query = _context.Drivers
            .Include(d => d.User)
            .Include(d => d.Event)
            .Include(d => d.Passengers)
                .ThenInclude(p => p.User)
            .AsQueryable();

        // Filter by type
        if (!string.IsNullOrWhiteSpace(TypeFilter) && TypeFilter != "All")
        {
            if (Enum.TryParse<DriverType>(TypeFilter, out var type))
            {
                query = query.Where(d => d.Type == type);
            }
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "All")
        {
            if (StatusFilter == "Active")
            {
                query = query.Where(d => d.IsActive && !d.IsMarkedByAdmin);
            }
            else if (StatusFilter == "Marked")
            {
                query = query.Where(d => d.IsMarkedByAdmin);
            }
            else if (StatusFilter == "Inactive")
            {
                query = query.Where(d => !d.IsActive);
            }
        }

        Drivers = await query
            .OrderBy(d => d.IsMarkedByAdmin)
            .ThenBy(d => d.CreatedAt)
            .ToListAsync();

        AllPassengers = await _context.Passengers
            .Include(p => p.User)
            .Include(p => p.Driver)
            .Include(p => p.Event)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostMarkDriverAsync(int id)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null)
        {
            Message = "Driver not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        driver.IsMarkedByAdmin = true;
        await _context.SaveChangesAsync();

        Message = $"Driver has been marked for security review.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnmarkDriverAsync(int id)
    {
        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null)
        {
            Message = "Driver not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        driver.IsMarkedByAdmin = false;
        await _context.SaveChangesAsync();

        Message = $"Driver has been unmarked.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReassignPassengerAsync(int passengerId, int newDriverId)
    {
        var passenger = await _context.Passengers
            .Include(p => p.Driver)
            .FirstOrDefaultAsync(p => p.Id == passengerId);
        
        if (passenger == null)
        {
            Message = "Passenger not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        var newDriver = await _context.Drivers
            .Include(d => d.Passengers)
            .FirstOrDefaultAsync(d => d.Id == newDriverId);

        if (newDriver == null)
        {
            Message = "Driver not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Check capacity
        var currentPassengerCount = newDriver.Passengers.Count;
        if (currentPassengerCount >= newDriver.Capacity)
        {
            Message = $"Cannot reassign: Driver {newDriver.User.Name} has reached capacity ({newDriver.Capacity}).";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Reassign
        passenger.DriverId = newDriverId;
        await _context.SaveChangesAsync();

        Message = $"Passenger has been reassigned successfully.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemovePassengerAsync(int passengerId)
    {
        var passenger = await _context.Passengers.FindAsync(passengerId);
        if (passenger == null)
        {
            Message = "Passenger not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        _context.Passengers.Remove(passenger);
        await _context.SaveChangesAsync();

        Message = $"Passenger has been removed from carpool.";
        IsSuccess = true;
        return RedirectToPage();
    }
}

