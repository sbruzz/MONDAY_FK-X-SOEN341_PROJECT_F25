using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Services;

namespace CampusEvents.Pages.Admin;

public class DriversModel : PageModel
{
    private readonly CarpoolService _carpoolService;
    private readonly AppDbContext _context;

    public DriversModel(CarpoolService carpoolService, AppDbContext context)
    {
        _carpoolService = carpoolService;
        _context = context;
    }

    public List<Driver> Drivers { get; set; } = new();
    public List<CarpoolPassenger> AllPassengers { get; set; } = new();

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
            return RedirectToPage("/Login");

        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
            return RedirectToPage("/Index");

        // Load all drivers with related data
        var query = _context.Drivers
            .Include(d => d.User)
            .Include(d => d.CarpoolOffers)
                .ThenInclude(o => o.Passengers)
                    .ThenInclude(p => p.Passenger)
            .AsQueryable();

        // Filter by type
        if (!string.IsNullOrWhiteSpace(TypeFilter) && TypeFilter != "All")
        {
            if (Enum.TryParse<DriverType>(TypeFilter, out var type))
            {
                query = query.Where(d => d.DriverType == type);
            }
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "All")
        {
            if (StatusFilter == "Active")
            {
                query = query.Where(d => d.Status == DriverStatus.Active && !d.SecurityFlags.Contains("flagged") && !d.SecurityFlags.Contains("marked"));
            }
            else if (StatusFilter == "Marked")
            {
                query = query.Where(d => d.SecurityFlags.Contains("flagged") || d.SecurityFlags.Contains("marked"));
            }
            else if (StatusFilter == "Inactive")
            {
                query = query.Where(d => d.Status != DriverStatus.Active);
            }
        }

        Drivers = await query
            .OrderBy(d => d.IsMarkedByAdmin)
            .ThenByDescending(d => d.CreatedAt)
            .ToListAsync();

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

        Message = result.Message;
        IsSuccess = result.Success;
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
            Message = "Suspension reason is required.";
            IsSuccess = false;
            return RedirectToPage();
        }

        var result = await _carpoolService.SuspendDriverAsync(driverId, reason);

        Message = result.Message;
        IsSuccess = result.Success;
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

        Message = result.Message;
        IsSuccess = result.Success;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkDriverAsync(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null)
        {
            Message = "Driver not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Add "marked" flag to SecurityFlags
        if (!driver.SecurityFlags.Contains("marked"))
        {
            driver.SecurityFlags = string.IsNullOrEmpty(driver.SecurityFlags)
                ? "marked"
                : driver.SecurityFlags + ",marked";
        }

        await _context.SaveChangesAsync();

        Message = $"Driver has been marked for security review.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnmarkDriverAsync(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var driver = await _context.Drivers.FindAsync(id);
        if (driver == null)
        {
            Message = "Driver not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Remove "marked" and "flagged" from SecurityFlags
        driver.SecurityFlags = driver.SecurityFlags
            .Replace("marked", "")
            .Replace("flagged", "")
            .Replace(",,", ",")
            .Trim(',');

        await _context.SaveChangesAsync();

        Message = $"Driver has been unmarked.";
        IsSuccess = true;
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

        Message = result.Message;
        IsSuccess = result.Success;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemovePassengerAsync(int passengerId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToPage("/Login");

        var passenger = await _context.CarpoolPassengers
            .Include(p => p.Offer)
            .FirstOrDefaultAsync(p => p.Id == passengerId);

        if (passenger == null)
        {
            Message = "Passenger not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Remove passenger and increment seats
        var offer = passenger.Offer;
        _context.CarpoolPassengers.Remove(passenger);
        offer.SeatsAvailable++;

        if (offer.Status == CarpoolOfferStatus.Full)
            offer.Status = CarpoolOfferStatus.Active;

        await _context.SaveChangesAsync();

        Message = $"Passenger has been removed from carpool.";
        IsSuccess = true;
        return RedirectToPage();
    }
}
