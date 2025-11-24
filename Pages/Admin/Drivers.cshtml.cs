using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Services;

namespace CampusEvents.Pages.Admin;

public class DriversModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly EncryptionService _encryptionService;

    public DriversModel(AppDbContext context, EncryptionService encryptionService)
    {
        _context = context;
        _encryptionService = encryptionService;
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
            .Include(d => d.CarpoolOffers)
                .ThenInclude(o => o.Event)
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
            if (StatusFilter == "Pending")
            {
                query = query.Where(d => d.Status == DriverStatus.Pending);
            }
            else if (StatusFilter == "Active")
            {
                query = query.Where(d => d.Status == DriverStatus.Active && !d.SecurityFlags.Contains("flagged") && !d.SecurityFlags.Contains("suspended"));
            }
            else if (StatusFilter == "Marked")
            {
                query = query.Where(d => d.SecurityFlags.Contains("flagged") || d.SecurityFlags.Contains("suspended"));
            }
            else if (StatusFilter == "Inactive")
            {
                query = query.Where(d => d.Status != DriverStatus.Active);
            }
        }

        Drivers = await query
            .OrderBy(d => d.SecurityFlags)
            .ThenBy(d => d.CreatedAt)
            .ToListAsync();

        AllPassengers = await _context.CarpoolPassengers
            .Include(p => p.Passenger)
            .Include(p => p.Offer)
                .ThenInclude(o => o.Driver)
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
        var passenger = await _context.CarpoolPassengers
            .Include(p => p.Offer)
                .ThenInclude(o => o.Driver)
            .FirstOrDefaultAsync(p => p.Id == passengerId);
        
        if (passenger == null)
        {
            Message = "Passenger not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        var newDriver = await _context.Drivers
            .Include(d => d.CarpoolOffers)
                .ThenInclude(o => o.Passengers)
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

        // Reassign - need to create a new carpool offer or add to existing one for the new driver
        // For simplicity, find an active offer from the new driver or create one
        var newOffer = newDriver.CarpoolOffers.FirstOrDefault(o => o.Status == CarpoolOfferStatus.Active && o.SeatsAvailable > 0);
        if (newOffer == null)
        {
            Message = "Cannot reassign: Driver has no active carpool offers with available seats.";
            IsSuccess = false;
            return RedirectToPage();
        }

        passenger.OfferId = newOffer.Id;
        newOffer.SeatsAvailable--;
        await _context.SaveChangesAsync();

        Message = $"Passenger has been reassigned successfully.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemovePassengerAsync(int passengerId)
    {
        var passenger = await _context.CarpoolPassengers
            .Include(p => p.Offer)
            .FirstOrDefaultAsync(p => p.Id == passengerId);
        if (passenger == null)
        {
            Message = "Passenger not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Free up the seat in the offer
        passenger.Offer.SeatsAvailable++;
        _context.CarpoolPassengers.Remove(passenger);
        await _context.SaveChangesAsync();

        Message = $"Passenger has been removed from carpool.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivateDriverAsync(int id)
    {
        var driver = await _context.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (driver == null)
        {
            Message = "Driver not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        driver.IsActive = true;
        await _context.SaveChangesAsync();

        Message = $"Driver {driver.User.Name} has been activated.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateDriverAsync(int id)
    {
        var driver = await _context.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (driver == null)
        {
            Message = "Driver not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        driver.IsActive = false;
        await _context.SaveChangesAsync();

        Message = $"Driver {driver.User.Name} has been deactivated.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostApproveDriverAsync(int id)
    {
        var driver = await _context.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (driver == null)
        {
            Message = "Driver not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        driver.Status = DriverStatus.Active;
        driver.SecurityFlags = string.IsNullOrEmpty(driver.SecurityFlags)
            ? "verified"
            : driver.SecurityFlags + ",verified";

        await _context.SaveChangesAsync();

        Message = $"Driver {driver.User.Name} has been approved and activated.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDenyDriverAsync(int id)
    {
        var driver = await _context.Drivers
            .Include(d => d.User)
            .Include(d => d.CarpoolOffers)
                .ThenInclude(o => o.Passengers)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (driver == null)
        {
            Message = "Driver not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        driver.Status = DriverStatus.Suspended;
        driver.SecurityFlags = string.IsNullOrEmpty(driver.SecurityFlags)
            ? "denied"
            : driver.SecurityFlags + ",denied";

        // Remove all passengers from this driver's carpool offers
        foreach (var offer in driver.CarpoolOffers)
        {
            var passengers = await _context.CarpoolPassengers
                .Where(p => p.OfferId == offer.Id)
                .ToListAsync();
            _context.CarpoolPassengers.RemoveRange(passengers);
            offer.Status = CarpoolOfferStatus.Cancelled;
        }

        await _context.SaveChangesAsync();

        Message = $"Driver {driver.User.Name} has been denied and suspended.";
        IsSuccess = true;
        return RedirectToPage();
    }

    /// <summary>
    /// Gets the decrypted driver's license number for display (admin only)
    /// </summary>
    public string GetDecryptedLicenseNumber(Driver driver)
    {
        if (string.IsNullOrEmpty(driver.DriverLicenseNumber))
            return "N/A";

        try
        {
            return _encryptionService.DecryptLicenseNumber(driver.DriverLicenseNumber);
        }
        catch
        {
            return "[Decryption Error]";
        }
    }

    /// <summary>
    /// Gets the decrypted license plate for display (admin only)
    /// </summary>
    public string GetDecryptedLicensePlate(Driver driver)
    {
        if (string.IsNullOrEmpty(driver.LicensePlate))
            return "N/A";

        try
        {
            return _encryptionService.DecryptLicensePlate(driver.LicensePlate);
        }
        catch
        {
            return "[Decryption Error]";
        }
    }
}

