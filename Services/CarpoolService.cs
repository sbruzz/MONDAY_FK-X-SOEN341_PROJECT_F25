using CampusEvents.Data;
using CampusEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEvents.Services;

/// <summary>
/// Service for managing carpool system (US.04)
/// Handles driver registration, ride offers, and passenger assignments
/// </summary>
public class CarpoolService
{
    private readonly AppDbContext _context;

    public CarpoolService(AppDbContext context)
    {
        _context = context;
    }

    // ===== Driver Registration =====

    /// <summary>
    /// Register a user as a driver
    /// Checks eligibility and creates driver profile
    /// </summary>
    public async Task<(bool Success, string Message, Driver? Driver)> RegisterDriverAsync(
        int userId,
        int capacity,
        VehicleType vehicleType,
        DriverType driverType,
        string? licensePlate = null,
        string accessibilityFeatures = "")
    {
        // Check if user exists
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found", null);

        // Check if user is already a driver
        var existingDriver = await _context.Drivers
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (existingDriver != null)
            return (false, "User is already registered as a driver", null);

        // Validate driver type matches user role
        if (driverType == DriverType.Student && user.Role != UserRole.Student)
            return (false, "Only students can register as student drivers", null);

        if (driverType == DriverType.Organizer && user.Role != UserRole.Organizer)
            return (false, "Only organizers can register as organizer drivers", null);

        // Validate capacity
        if (capacity < 1 || capacity > 50)
            return (false, "Capacity must be between 1 and 50", null);

        // Create driver profile
        var driver = new Driver
        {
            UserId = userId,
            Capacity = capacity,
            VehicleType = vehicleType,
            DriverType = driverType,
            LicensePlate = licensePlate,
            Status = DriverStatus.Pending, // Requires admin approval
            AccessibilityFeatures = accessibilityFeatures,
            SecurityFlags = string.Empty,
            History = string.Empty
        };

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        return (true, "Driver registration successful. Pending admin approval.", driver);
    }

    /// <summary>
    /// Update driver profile
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateDriverAsync(
        int driverId,
        int? capacity = null,
        VehicleType? vehicleType = null,
        string? licensePlate = null,
        string? accessibilityFeatures = null)
    {
        var driver = await _context.Drivers.FindAsync(driverId);
        if (driver == null)
            return (false, "Driver not found");

        if (capacity.HasValue)
        {
            if (capacity.Value < 1 || capacity.Value > 50)
                return (false, "Capacity must be between 1 and 50");
            driver.Capacity = capacity.Value;
        }

        if (vehicleType.HasValue)
            driver.VehicleType = vehicleType.Value;

        if (licensePlate != null)
            driver.LicensePlate = licensePlate;

        if (accessibilityFeatures != null)
            driver.AccessibilityFeatures = accessibilityFeatures;

        await _context.SaveChangesAsync();
        return (true, "Driver profile updated successfully");
    }

    // ===== Carpool Offer Management =====

    /// <summary>
    /// Create a carpool offer for an event
    /// </summary>
    public async Task<(bool Success, string Message, CarpoolOffer? Offer)> CreateOfferAsync(
        int driverId,
        int eventId,
        string departureInfo,
        DateTime departureTime,
        string? departureAddress = null,
        double? latitude = null,
        double? longitude = null)
    {
        // Validate driver exists and is active
        var driver = await _context.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == driverId);

        if (driver == null)
            return (false, "Driver not found", null);

        if (driver.Status != DriverStatus.Active)
            return (false, "Driver account is not active. Contact administrator.", null);

        // Validate event exists
        var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists)
            return (false, "Event not found", null);

        // Check if driver already has an active offer for this event
        var existingOffer = await _context.CarpoolOffers
            .FirstOrDefaultAsync(co => co.DriverId == driverId &&
                                      co.EventId == eventId &&
                                      co.Status == CarpoolOfferStatus.Active);

        if (existingOffer != null)
            return (false, "You already have an active offer for this event", null);

        // Create carpool offer
        var offer = new CarpoolOffer
        {
            DriverId = driverId,
            EventId = eventId,
            SeatsAvailable = driver.Capacity,
            DepartureInfo = departureInfo,
            DepartureAddress = departureAddress,
            Latitude = latitude,
            Longitude = longitude,
            DepartureTime = departureTime,
            Status = CarpoolOfferStatus.Active
        };

        _context.CarpoolOffers.Add(offer);
        await _context.SaveChangesAsync();

        return (true, "Carpool offer created successfully", offer);
    }

    /// <summary>
    /// Get all active carpool offers for an event
    /// </summary>
    public async Task<List<CarpoolOffer>> GetEventOffersAsync(int eventId)
    {
        return await _context.CarpoolOffers
            .Include(co => co.Driver)
                .ThenInclude(d => d.User)
            .Include(co => co.Passengers)
            .Where(co => co.EventId == eventId && co.Status == CarpoolOfferStatus.Active)
            .OrderByDescending(co => co.SeatsAvailable)
            .ToListAsync();
    }

    /// <summary>
    /// Cancel a carpool offer
    /// </summary>
    public async Task<(bool Success, string Message)> CancelOfferAsync(int offerId, int userId)
    {
        var offer = await _context.CarpoolOffers
            .Include(co => co.Driver)
            .Include(co => co.Passengers)
            .FirstOrDefaultAsync(co => co.Id == offerId);

        if (offer == null)
            return (false, "Offer not found");

        // Verify user is the driver
        if (offer.Driver.UserId != userId)
            return (false, "Only the driver can cancel this offer");

        // Check if there are confirmed passengers
        var confirmedPassengers = offer.Passengers.Count(p => p.Status == PassengerStatus.Confirmed);
        if (confirmedPassengers > 0)
            return (false, $"Cannot cancel: {confirmedPassengers} passengers have confirmed. Please contact them first.");

        offer.Status = CarpoolOfferStatus.Cancelled;
        await _context.SaveChangesAsync();

        return (true, "Carpool offer cancelled successfully");
    }

    // ===== Passenger Management =====

    /// <summary>
    /// Passenger joins a carpool offer
    /// </summary>
    public async Task<(bool Success, string Message)> JoinOfferAsync(
        int offerId,
        int passengerId,
        string? pickupLocation = null,
        string? notes = null)
    {
        // Validate offer exists and is active
        var offer = await _context.CarpoolOffers
            .Include(co => co.Driver)
            .Include(co => co.Passengers)
            .FirstOrDefaultAsync(co => co.Id == offerId);

        if (offer == null)
            return (false, "Carpool offer not found");

        if (offer.Status != CarpoolOfferStatus.Active)
            return (false, "This carpool offer is no longer active");

        // Check seats available
        if (offer.SeatsAvailable <= 0)
            return (false, "No seats available");

        // Check if passenger is the driver
        if (offer.Driver.UserId == passengerId)
            return (false, "You cannot join your own carpool offer");

        // Check if passenger already joined
        var existingPassenger = await _context.CarpoolPassengers
            .FirstOrDefaultAsync(cp => cp.OfferId == offerId &&
                                      cp.PassengerId == passengerId &&
                                      cp.Status != PassengerStatus.Cancelled);

        if (existingPassenger != null)
            return (false, "You have already joined this carpool");

        // Create passenger assignment
        var passenger = new CarpoolPassenger
        {
            OfferId = offerId,
            PassengerId = passengerId,
            Status = PassengerStatus.Confirmed,
            PickupLocation = pickupLocation,
            Notes = notes
        };

        _context.CarpoolPassengers.Add(passenger);

        // Decrement available seats
        offer.SeatsAvailable--;

        // Mark as full if no more seats
        if (offer.SeatsAvailable == 0)
            offer.Status = CarpoolOfferStatus.Full;

        await _context.SaveChangesAsync();

        return (true, "Successfully joined carpool");
    }

    /// <summary>
    /// Passenger leaves a carpool offer
    /// </summary>
    public async Task<(bool Success, string Message)> LeaveOfferAsync(int offerId, int passengerId)
    {
        var passenger = await _context.CarpoolPassengers
            .Include(cp => cp.Offer)
            .FirstOrDefaultAsync(cp => cp.OfferId == offerId &&
                                      cp.PassengerId == passengerId &&
                                      cp.Status == PassengerStatus.Confirmed);

        if (passenger == null)
            return (false, "You are not part of this carpool");

        // Cancel passenger assignment
        passenger.Status = PassengerStatus.Cancelled;

        // Increment available seats
        var offer = passenger.Offer;
        offer.SeatsAvailable++;

        // Reactivate offer if it was full
        if (offer.Status == CarpoolOfferStatus.Full)
            offer.Status = CarpoolOfferStatus.Active;

        await _context.SaveChangesAsync();

        return (true, "Successfully left carpool");
    }

    /// <summary>
    /// Get user's carpool participations (as driver or passenger)
    /// </summary>
    public async Task<(List<CarpoolOffer> AsDriver, List<CarpoolPassenger> AsPassenger)> GetUserCarpoolsAsync(int userId)
    {
        // Get driver's offers
        var driver = await _context.Drivers
            .FirstOrDefaultAsync(d => d.UserId == userId);

        var asDriver = new List<CarpoolOffer>();
        if (driver != null)
        {
            asDriver = await _context.CarpoolOffers
                .Include(co => co.Event)
                .Include(co => co.Passengers)
                    .ThenInclude(p => p.Passenger)
                .Where(co => co.DriverId == driver.Id)
                .OrderByDescending(co => co.CreatedAt)
                .ToListAsync();
        }

        // Get passenger assignments
        var asPassenger = await _context.CarpoolPassengers
            .Include(cp => cp.Offer)
                .ThenInclude(o => o.Event)
            .Include(cp => cp.Offer)
                .ThenInclude(o => o.Driver)
                    .ThenInclude(d => d.User)
            .Where(cp => cp.PassengerId == userId && cp.Status != PassengerStatus.Cancelled)
            .OrderByDescending(cp => cp.JoinedAt)
            .ToListAsync();

        return (asDriver, asPassenger);
    }

    // ===== Admin Functions =====

    /// <summary>
    /// Admin approves a driver
    /// </summary>
    public async Task<(bool Success, string Message)> ApproveDriverAsync(int driverId)
    {
        var driver = await _context.Drivers.FindAsync(driverId);
        if (driver == null)
            return (false, "Driver not found");

        driver.Status = DriverStatus.Active;
        await _context.SaveChangesAsync();

        return (true, "Driver approved successfully");
    }

    /// <summary>
    /// Admin suspends a driver
    /// </summary>
    public async Task<(bool Success, string Message)> SuspendDriverAsync(int driverId, string reason)
    {
        var driver = await _context.Drivers
            .Include(d => d.CarpoolOffers)
            .FirstOrDefaultAsync(d => d.Id == driverId);

        if (driver == null)
            return (false, "Driver not found");

        driver.Status = DriverStatus.Suspended;

        // Add security flag
        if (!string.IsNullOrEmpty(driver.SecurityFlags))
            driver.SecurityFlags += ",";
        driver.SecurityFlags += $"suspended:{DateTime.UtcNow:yyyy-MM-dd}:{reason}";

        // Cancel all active offers
        foreach (var offer in driver.CarpoolOffers.Where(o => o.Status == CarpoolOfferStatus.Active))
        {
            offer.Status = CarpoolOfferStatus.Cancelled;
        }

        await _context.SaveChangesAsync();

        return (true, "Driver suspended successfully");
    }

    /// <summary>
    /// Admin unsuspends a driver
    /// </summary>
    public async Task<(bool Success, string Message)> UnsuspendDriverAsync(int driverId)
    {
        var driver = await _context.Drivers.FindAsync(driverId);
        if (driver == null)
            return (false, "Driver not found");

        driver.Status = DriverStatus.Active;
        await _context.SaveChangesAsync();

        return (true, "Driver unsuspended successfully");
    }

    /// <summary>
    /// Get all drivers (for admin)
    /// </summary>
    public async Task<List<Driver>> GetAllDriversAsync(DriverStatus? status = null)
    {
        var query = _context.Drivers
            .Include(d => d.User)
            .Include(d => d.CarpoolOffers)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get flagged drivers (for admin review)
    /// </summary>
    public async Task<List<Driver>> GetFlaggedDriversAsync()
    {
        return await _context.Drivers
            .Include(d => d.User)
            .Where(d => d.SecurityFlags.Contains("flagged") || d.SecurityFlags.Contains("suspended"))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Admin reassigns a passenger from one carpool offer to another
    /// </summary>
    public async Task<(bool Success, string Message)> ReassignPassengerAsync(
        int passengerId,
        int newOfferId)
    {
        // Get the passenger
        var passenger = await _context.CarpoolPassengers
            .Include(cp => cp.Offer)
                .ThenInclude(o => o.Event)
            .FirstOrDefaultAsync(cp => cp.Id == passengerId);

        if (passenger == null)
            return (false, "Passenger record not found");

        // Get the new offer
        var newOffer = await _context.CarpoolOffers
            .Include(o => o.Event)
            .Include(o => o.Passengers)
            .FirstOrDefaultAsync(o => o.Id == newOfferId);

        if (newOffer == null)
            return (false, "Target carpool offer not found");

        // Validate offers are for the same event
        if (passenger.Offer.EventId != newOffer.EventId)
            return (false, "Cannot reassign passenger to a carpool for a different event");

        // Check if new offer has available seats
        if (newOffer.SeatsAvailable <= 0)
            return (false, "Target carpool offer is full");

        // Check if new offer is active
        if (newOffer.Status != CarpoolOfferStatus.Active && newOffer.Status != CarpoolOfferStatus.Full)
            return (false, "Target carpool offer is not active");

        var oldOfferId = passenger.OfferId;

        // Update passenger's offer
        passenger.OfferId = newOfferId;
        passenger.Status = PassengerStatus.Confirmed; // Auto-confirm on admin reassignment

        // Decrement seats in new offer
        newOffer.SeatsAvailable--;
        if (newOffer.SeatsAvailable == 0)
            newOffer.Status = CarpoolOfferStatus.Full;

        // Increment seats in old offer
        var oldOffer = await _context.CarpoolOffers.FindAsync(oldOfferId);
        if (oldOffer != null)
        {
            oldOffer.SeatsAvailable++;
            if (oldOffer.Status == CarpoolOfferStatus.Full)
                oldOffer.Status = CarpoolOfferStatus.Active;
        }

        await _context.SaveChangesAsync();

        return (true, $"Passenger reassigned successfully to the new carpool");
    }

    /// <summary>
    /// Get all carpool passengers (for admin)
    /// </summary>
    public async Task<List<CarpoolPassenger>> GetAllPassengersAsync(int? eventId = null)
    {
        var query = _context.CarpoolPassengers
            .Include(cp => cp.Passenger)
            .Include(cp => cp.Offer)
                .ThenInclude(o => o.Event)
            .Include(cp => cp.Offer)
                .ThenInclude(o => o.Driver)
                    .ThenInclude(d => d.User)
            .AsQueryable();

        if (eventId.HasValue)
            query = query.Where(cp => cp.Offer.EventId == eventId.Value);

        return await query
            .OrderByDescending(cp => cp.JoinedAt)
            .ToListAsync();
    }
}
