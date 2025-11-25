namespace CampusEvents.Models;

/// <summary>
/// Status of a room rental booking.
/// Tracks the approval and lifecycle state of a room rental request.
/// </summary>
/// <remarks>
/// Rental status workflow:
/// - Pending: Initial state, awaiting organizer/admin approval
/// - Approved: Approved by organizer/admin, rental confirmed
/// - Rejected: Rejected by organizer/admin, rental denied
/// - Completed: Rental period has passed, rental completed
/// - Cancelled: Rental cancelled by renter or admin
/// 
/// Status Transitions:
/// - Pending → Approved: Organizer/admin approves
/// - Pending → Rejected: Organizer/admin rejects
/// - Pending → Cancelled: Renter cancels before approval
/// - Approved → Cancelled: Renter or admin cancels after approval
/// - Approved → Completed: Rental period ends
/// </remarks>
public enum RentalStatus
{
    /// <summary>
    /// Pending approval - awaiting organizer/admin review
    /// </summary>
    Pending,
    
    /// <summary>
    /// Approved rental - confirmed and active
    /// </summary>
    Approved,
    
    /// <summary>
    /// Rejected rental - denied by organizer/admin
    /// </summary>
    Rejected,
    
    /// <summary>
    /// Completed rental - rental period has passed
    /// </summary>
    Completed,
    
    /// <summary>
    /// Cancelled rental - cancelled by renter or admin
    /// </summary>
    Cancelled
}

/// <summary>
/// Room rental booking entity.
/// Represents a request to rent a room for a specific time period, with approval workflow
/// and double booking prevention.
/// </summary>
/// <remarks>
/// The RoomRental entity represents a booking request for a room, including time period,
/// purpose, expected attendees, and calculated cost. It implements a comprehensive
/// approval workflow and prevents double booking through overlap detection.
/// 
/// Key Features:
/// - Rental request creation with time period
/// - Approval workflow (Pending → Approved/Rejected)
/// - Double booking prevention (overlap detection)
/// - Capacity validation
/// - Cost calculation (if hourly rate set)
/// - Cancellation support
/// 
/// Business Rules:
/// - Start time must be in the future
/// - End time must be after start time
/// - Cannot overlap with existing Approved/Pending rentals for same room
/// - Expected attendees cannot exceed room capacity
/// - Must be within room availability window (if specified)
/// - TotalCost calculated from HourlyRate and duration (if rate set)
/// - Requires organizer/admin approval
/// 
/// Double Booking Prevention:
/// - Composite index on (RoomId, StartTime, EndTime) for efficient queries
/// - Overlap detection checks for:
///   - Start time within existing rental period
///   - End time within existing rental period
///   - Rental period completely contains existing rental
/// - Only Approved and Pending rentals block new rentals
/// - Re-checked on approval to prevent race conditions
/// 
/// Cost Calculation:
/// - If room has HourlyRate, TotalCost = (EndTime - StartTime).TotalHours * HourlyRate
/// - If room has no HourlyRate, TotalCost is null (free rental)
/// - Calculated at rental request time
/// 
/// Approval Workflow:
/// - Rental created with Status.Pending
/// - Organizer or admin reviews request
/// - Approved: Rental confirmed, notification sent to renter
/// - Rejected: Rental denied, notification sent with reason
/// 
/// Relationships:
/// - Many-to-One: RoomRental → Room (rental is for a room)
/// - Many-to-One: RoomRental → User (user who is renting)
/// 
/// Backward Compatibility:
/// This class includes NotMapped properties for backward compatibility with
/// older UI code that may reference properties by different names.
/// 
/// Example Usage:
/// ```csharp
/// // Request room rental
/// var rental = new RoomRental
/// {
///     RoomId = roomId,
///     RenterId = userId,
///     StartTime = DateTime.UtcNow.AddDays(3).AddHours(14), // 2:00 PM
///     EndTime = DateTime.UtcNow.AddDays(3).AddHours(17),   // 5:00 PM
///     Purpose = "Study group session",
///     ExpectedAttendees = 15,
///     Status = RentalStatus.Pending, // Requires approval
///     TotalCost = 75.00m // 3 hours * $25/hour
/// };
/// ```
/// </remarks>
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

    /// <summary>
    /// Optional event ID if rental is for an event (legacy)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int? EventId { get; set; }

    /// <summary>
    /// Optional event if rental is for an event (legacy)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Event? Event { get; set; }

    // Backward compatibility

    /// <summary>
    /// Alias for Renter (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public User User => Renter;

    /// <summary>
    /// Alias for RenterId (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int UserId
    {
        get => RenterId;
        set => RenterId = value;
    }
}
