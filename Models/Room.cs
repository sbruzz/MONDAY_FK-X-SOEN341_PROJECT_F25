namespace CampusEvents.Models;

/// <summary>
/// Room status managed by administrators
/// </summary>
public enum RoomStatus
{
    Enabled,
    Disabled,
    UnderMaintenance
}

/// <summary>
/// Room entity for rental system (Task.42)
/// Organizers can create rooms that students/organizers can rent
/// </summary>
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
