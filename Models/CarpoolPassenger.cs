namespace CampusEvents.Models;

/// <summary>
/// Status of a passenger's carpool assignment
/// </summary>
public enum PassengerStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
}

/// <summary>
/// Passenger assignment to a carpool offer
/// Tracks which users have joined which rides
/// </summary>
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
