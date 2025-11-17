namespace CampusEvents.Models;

/// <summary>
/// Status of a carpool offer
/// </summary>
public enum CarpoolOfferStatus
{
    Active,
    Full,
    Cancelled,
    Completed
}

/// <summary>
/// Carpool offer for an event
/// Drivers create offers with available seats for passengers
/// </summary>
public class CarpoolOffer
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Event table
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// Foreign key to Driver table
    /// </summary>
    public int DriverId { get; set; }

    /// <summary>
    /// Number of seats currently available
    /// Decrements when passengers join
    /// </summary>
    public int SeatsAvailable { get; set; }

    /// <summary>
    /// Departure information (time, location details)
    /// </summary>
    public required string DepartureInfo { get; set; }

    /// <summary>
    /// Departure address (for proximity calculations)
    /// </summary>
    public string? DepartureAddress { get; set; }

    /// <summary>
    /// Latitude for geo-proximity (optional)
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude for geo-proximity (optional)
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Estimated departure time
    /// </summary>
    public DateTime DepartureTime { get; set; }

    /// <summary>
    /// Current status of offer
    /// </summary>
    public CarpoolOfferStatus Status { get; set; } = CarpoolOfferStatus.Active;

    /// <summary>
    /// When offer was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    /// <summary>
    /// Associated event
    /// </summary>
    public Event Event { get; set; } = null!;

    /// <summary>
    /// Driver who created this offer
    /// </summary>
    public Driver Driver { get; set; } = null!;

    /// <summary>
    /// Passengers who joined this ride
    /// </summary>
    public ICollection<CarpoolPassenger> Passengers { get; set; } = new List<CarpoolPassenger>();
}
