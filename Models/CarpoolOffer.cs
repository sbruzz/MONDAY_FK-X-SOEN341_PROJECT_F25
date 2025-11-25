namespace CampusEvents.Models;

/// <summary>
/// Status of a carpool offer.
/// Tracks the current state of a carpool ride offer.
/// </summary>
/// <remarks>
/// Carpool offer status workflow:
/// - Active: Offer is accepting passengers, seats available
/// - Full: All seats taken, no more passengers can join
/// - Cancelled: Offer cancelled by driver
/// - Completed: Ride has been completed
/// 
/// Status Transitions:
/// - Active → Full: When SeatsAvailable reaches 0
/// - Active → Cancelled: Driver cancels offer
/// - Full → Active: Passenger leaves, seat becomes available
/// - Any → Completed: After event/ride completion
/// </remarks>
public enum CarpoolOfferStatus
{
    /// <summary>
    /// Active offer - accepting passengers, seats available
    /// </summary>
    Active,
    
    /// <summary>
    /// Full offer - all seats taken, no more passengers can join
    /// </summary>
    Full,
    
    /// <summary>
    /// Cancelled offer - driver cancelled the ride
    /// </summary>
    Cancelled,
    
    /// <summary>
    /// Completed offer - ride has been completed
    /// </summary>
    Completed
}

/// <summary>
/// Carpool offer for an event.
/// Represents a driver's offer to provide transportation to an event with available seats for passengers.
/// </summary>
/// <remarks>
/// The CarpoolOffer entity represents a driver's willingness to provide transportation
/// to an event, with a specified number of available seats for passengers to join.
/// 
/// Key Features:
/// - Driver creates offer for specific event
/// - Tracks available seats (decrements as passengers join)
/// - Departure information (time, location, address)
/// - Optional geolocation (latitude/longitude) for proximity matching
/// - Status management (Active, Full, Cancelled, Completed)
/// 
/// Business Rules:
/// - Only active drivers can create offers
/// - One active offer per driver per event
/// - SeatsAvailable initialized from driver's capacity
/// - SeatsAvailable decrements when passengers join
/// - SeatsAvailable increments when passengers leave
/// - Status changes to "Full" when SeatsAvailable reaches 0
/// - Status changes to "Active" when seat becomes available (from Full)
/// - Offer can be cancelled if no confirmed passengers
/// 
/// Seat Management:
/// - SeatsAvailable tracks current available seats
/// - Initialized from driver.Capacity when offer is created
/// - Automatically decremented when passenger joins
/// - Automatically incremented when passenger leaves
/// - Status updated based on seat availability
/// 
/// Geolocation:
/// - Optional Latitude/Longitude for proximity calculations
/// - Used by ProximityService to find nearby offers
/// - Helps passengers find carpools close to their location
/// - Coordinates validated using ValidationHelper
/// 
/// Departure Information:
/// - DepartureInfo: Human-readable departure details (time, location)
/// - DepartureAddress: Physical address for mapping/navigation
/// - DepartureTime: Estimated departure time
/// - All used to help passengers plan their trip
/// 
/// Relationships:
/// - Many-to-One: CarpoolOffer → Event (offer is for an event)
/// - Many-to-One: CarpoolOffer → Driver (driver who created the offer)
/// - One-to-Many: CarpoolOffer → CarpoolPassenger (passengers who joined)
/// 
/// Example Usage:
/// ```csharp
/// // Create carpool offer
/// var offer = new CarpoolOffer
/// {
///     EventId = eventId,
///     DriverId = driverId,
///     SeatsAvailable = driver.Capacity, // Initialize from driver capacity
///     DepartureInfo = "Leaving from Hall Building at 9:00 AM",
///     DepartureAddress = "1455 De Maisonneuve Blvd W, Montreal, QC",
///     Latitude = 45.4972,
///     Longitude = -73.5794,
///     DepartureTime = DateTime.UtcNow.AddDays(7).AddHours(-1), // 1 hour before event
///     Status = CarpoolOfferStatus.Active
/// };
/// ```
/// </remarks>
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
