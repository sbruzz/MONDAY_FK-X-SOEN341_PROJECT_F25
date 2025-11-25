namespace CampusEvents.Models;

/// <summary>
/// Room status managed by administrators.
/// Controls room availability and rental eligibility.
/// </summary>
/// <remarks>
/// Room status workflow:
/// - Enabled: Room is available for rental (default state)
/// - Disabled: Room disabled by admin (overrides all rentals, cancels pending)
/// - UnderMaintenance: Room temporarily unavailable (maintenance, repairs)
/// 
/// Status Effects:
/// - Enabled: Room can be rented, all rentals proceed normally
/// - Disabled: No new rentals allowed, pending rentals rejected, approved rentals notified
/// - UnderMaintenance: Similar to disabled, but temporary
/// </remarks>
public enum RoomStatus
{
    /// <summary>
    /// Room is enabled and available for rental
    /// </summary>
    Enabled,
    
    /// <summary>
    /// Room is disabled by administrator (overrides all rental status)
    /// </summary>
    Disabled,
    
    /// <summary>
    /// Room is under maintenance and temporarily unavailable
    /// </summary>
    UnderMaintenance
}

/// <summary>
/// Room entity for the room rental system (User Story US.04, Task.42).
/// Represents a rentable room that organizers can create and students/organizers can rent.
/// </summary>
/// <remarks>
/// The Room entity represents a physical space that can be rented for events, meetings,
/// or other purposes. Rooms are created by organizers and can be rented by students
/// or other organizers.
/// 
/// Key Features:
/// - Room creation and management by organizers
/// - Capacity management
/// - Availability window configuration
/// - Amenities listing
/// - Pricing support (hourly rates)
/// - Administrative controls (enable/disable)
/// 
/// Business Rules:
/// - Only organizers can create rooms
/// - Capacity must be positive
/// - AvailabilityEnd must be after AvailabilityStart if both provided
/// - HourlyRate is optional (free rooms supported)
/// - Status can be changed by admin (overrides all rentals)
/// - Disabled rooms reject all pending rentals
/// 
/// Availability Windows:
/// - Optional start and end times for room availability
/// - If specified, rentals must be within this window
/// - Useful for rooms with restricted access hours
/// 
/// Amenities:
/// - Comma-separated list of available amenities
/// - Examples: "projector", "whiteboard", "wifi", "ac", "parking", "catering"
/// - Helps renters find suitable rooms
/// 
/// Pricing:
/// - HourlyRate: Optional hourly rental rate
/// - If set, TotalCost calculated for rentals based on duration
/// - Free rooms have null HourlyRate
/// 
/// Relationships:
/// - Many-to-One: Room → User (organizer who manages the room)
/// - One-to-Many: Room → RoomRental (rental bookings for this room)
/// 
/// Double Booking Prevention:
/// - Overlapping rentals prevented at rental request time
/// - Composite index on (RoomId, StartTime, EndTime) for efficient queries
/// - Only Approved and Pending rentals block new rentals
/// 
/// Backward Compatibility:
/// This class includes NotMapped properties for backward compatibility with
/// older UI code that may reference properties by different names.
/// 
/// Example Usage:
/// ```csharp
/// // Create a room
/// var room = new Room
/// {
///     OrganizerId = organizerId,
///     Name = "Conference Room A",
///     Address = "Hall Building, Room H-110",
///     Capacity = 50,
///     RoomInfo = "Large conference room with projector and whiteboard",
///     Amenities = "projector,whiteboard,wifi,ac",
///     HourlyRate = 25.00m,
///     Status = RoomStatus.Enabled,
///     AvailabilityStart = DateTime.UtcNow,
///     AvailabilityEnd = DateTime.UtcNow.AddDays(90)
/// };
/// ```
/// </remarks>
public class Room
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to User table (organizer who owns/manages this room)
    /// </summary>
    public int OrganizerId { get; set; }

    /// <summary>
    /// Room name/identifier
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Physical address of the room
    /// </summary>
    public required string Address { get; set; }

    /// <summary>
    /// Detailed information about the room
    /// </summary>
    public string? RoomInfo { get; set; }

    /// <summary>
    /// Maximum capacity of the room
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Room status (Enabled/Disabled by admin)
    /// Admin disable overrides all rental status
    /// </summary>
    public RoomStatus Status { get; set; } = RoomStatus.Enabled;

    /// <summary>
    /// Availability start time (optional)
    /// </summary>
    public DateTime? AvailabilityStart { get; set; }

    /// <summary>
    /// Availability end time (optional)
    /// </summary>
    public DateTime? AvailabilityEnd { get; set; }

    /// <summary>
    /// Amenities available in the room
    /// Comma-separated: "projector", "whiteboard", "wifi", "ac"
    /// </summary>
    public string Amenities { get; set; } = string.Empty;

    /// <summary>
    /// Hourly rental rate (if applicable)
    /// </summary>
    public decimal? HourlyRate { get; set; }

    /// <summary>
    /// When room was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    /// <summary>
    /// Organizer who manages this room
    /// </summary>
    public User Organizer { get; set; } = null!;

    /// <summary>
    /// Rental bookings for this room
    /// </summary>
    public ICollection<RoomRental> Rentals { get; set; } = new List<RoomRental>();

    // Backward compatibility

    /// <summary>
    /// Alias for Address (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Location
    {
        get => Address;
        set => Address = value;
    }

    /// <summary>
    /// Alias for RoomInfo (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Description
    {
        get => RoomInfo;
        set => RoomInfo = value;
    }

    /// <summary>
    /// Computed property for backward compatibility
    /// Returns true if Status is Enabled
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsEnabled
    {
        get => Status == RoomStatus.Enabled;
        set => Status = value ? RoomStatus.Enabled : RoomStatus.Disabled;
    }
}
