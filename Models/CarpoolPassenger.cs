namespace CampusEvents.Models;

/// <summary>
/// Status of a passenger's carpool assignment.
/// Represents the current state of a passenger's participation in a carpool ride.
/// </summary>
/// <remarks>
/// Status transitions:
/// - Pending: Initial state when passenger joins (rarely used, typically goes directly to Confirmed)
/// - Confirmed: Passenger is confirmed for the ride
/// - Cancelled: Passenger cancelled their participation
/// - Completed: Ride has been completed
/// </remarks>
public enum PassengerStatus
{
    /// <summary>
    /// Passenger assignment is pending confirmation
    /// </summary>
    Pending,
    
    /// <summary>
    /// Passenger is confirmed for the ride
    /// </summary>
    Confirmed,
    
    /// <summary>
    /// Passenger cancelled their participation
    /// </summary>
    Cancelled,
    
    /// <summary>
    /// Ride has been completed
    /// </summary>
    Completed
}

/// <summary>
/// Passenger assignment to a carpool offer.
/// Represents the many-to-many relationship between Users and CarpoolOffers,
/// tracking which users have joined which carpool rides.
/// </summary>
/// <remarks>
/// This entity serves as a join table in the many-to-many relationship between:
/// - Users (passengers) and CarpoolOffers (rides)
/// 
/// Key Features:
/// - Tracks passenger status (Confirmed, Cancelled, etc.)
/// - Stores passenger-specific information (pickup location, notes)
/// - Records when passenger joined the ride
/// - Maintains relationship between passenger and carpool offer
/// 
/// Business Rules:
/// - One passenger can join multiple carpool offers (for different events)
/// - One passenger cannot join the same carpool offer twice
/// - When passenger joins, SeatsAvailable on CarpoolOffer is decremented
/// - When passenger leaves, SeatsAvailable on CarpoolOffer is incremented
/// - Passenger cannot be the driver of the same offer
/// 
/// Backward Compatibility:
/// This class includes several NotMapped properties for backward compatibility
/// with older UI code that may reference properties by different names.
/// </remarks>
public class CarpoolPassenger
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to CarpoolOffer table
    /// </summary>
    public int OfferId { get; set; }

    /// <summary>
    /// Foreign key to User table (the passenger)
    /// </summary>
    public int PassengerId { get; set; }

    /// <summary>
    /// Current status of passenger assignment
    /// </summary>
    public PassengerStatus Status { get; set; } = PassengerStatus.Pending;

    /// <summary>
    /// When passenger joined the ride
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional pickup location for this passenger
    /// </summary>
    public string? PickupLocation { get; set; }

    /// <summary>
    /// Optional notes from passenger
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties

    /// <summary>
    /// Associated carpool offer
    /// </summary>
    public CarpoolOffer Offer { get; set; } = null!;

    /// <summary>
    /// User who is the passenger
    /// </summary>
    public User Passenger { get; set; } = null!;

    // Backward compatibility

    /// <summary>
    /// Alias for Passenger (for backward compatibility with old UI code)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public User User => Passenger;

    /// <summary>
    /// Alias for PassengerId (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int UserId
    {
        get => PassengerId;
        set => PassengerId = value;
    }

    /// <summary>
    /// Legacy EventId from the associated offer (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int EventId
    {
        get => Offer?.EventId ?? 0;
        set { /* Ignored - EventId is set on CarpoolOffer */ }
    }

    /// <summary>
    /// Legacy DriverId from the associated offer (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int DriverId
    {
        get => Offer?.DriverId ?? 0;
        set { /* Ignored - DriverId is set on CarpoolOffer */ }
    }

    /// <summary>
    /// Alias for JoinedAt (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime AssignedAt
    {
        get => JoinedAt;
        set => JoinedAt = value;
    }

    /// <summary>
    /// Legacy Driver property from the associated offer (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Driver? Driver => Offer?.Driver;
}
