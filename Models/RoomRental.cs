namespace CampusEvents.Models;

/// <summary>
/// Status of a room rental booking
/// </summary>
public enum RentalStatus
{
    Pending,
    Approved,
    Rejected,
    Completed,
    Cancelled
}

/// <summary>
/// Room rental booking
/// Prevents double booking through status checks
/// </summary>
public class RoomRental
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Room table
    /// </summary>
    public int RoomId { get; set; }

    /// <summary>
    /// Foreign key to User table (the person renting)
    /// </summary>
    public int RenterId { get; set; }

    /// <summary>
    /// Rental start time
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Rental end time
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Current status of rental
    /// Used to prevent double booking (overlapping Approved/Pending rentals)
    /// </summary>
    public RentalStatus Status { get; set; } = RentalStatus.Pending;

    /// <summary>
    /// Purpose of rental
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Expected number of attendees
    /// </summary>
    public int? ExpectedAttendees { get; set; }

    /// <summary>
    /// Total cost calculated from room rate and duration
    /// </summary>
    public decimal? TotalCost { get; set; }

    /// <summary>
    /// When rental was requested
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Admin notes (for approval/rejection)
    /// </summary>
    public string? AdminNotes { get; set; }

    // Navigation properties

    /// <summary>
    /// Associated room
    /// </summary>
    public Room Room { get; set; } = null!;

    /// <summary>
    /// User who is renting the room
    /// </summary>
    public User Renter { get; set; } = null!;
}
