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
}
