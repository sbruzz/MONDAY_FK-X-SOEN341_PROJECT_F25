namespace CampusEvents.Models;

/// <summary>
/// Vehicle types available for carpooling
/// </summary>
public enum VehicleType
{
    Mini,
    Sedan,
    SUV,
    Van,
    Bus
}

/// <summary>
/// Driver status managed by administrators
/// </summary>
public enum DriverStatus
{
    Active,
    Suspended,
    Pending
}

/// <summary>
/// Driver type indicating who can be a driver
/// </summary>
public enum DriverType
{
    Student,
    Organizer
}

/// <summary>
/// Driver entity for carpool system (Task.36)
/// Manages driver profiles with vehicle information and admin controls
/// </summary>
public class Driver
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to User table
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// License plate number (optional)
    /// </summary>
    public string? LicensePlate { get; set; }

    /// <summary>
    /// Total passenger capacity (excluding driver)
    /// </summary>
    public required int Capacity { get; set; }

    /// <summary>
    /// Type of vehicle
    /// </summary>
    public VehicleType VehicleType { get; set; }

    /// <summary>
    /// Whether driver is Student or Organizer
    /// </summary>
    public DriverType DriverType { get; set; }

    /// <summary>
    /// Current status (Active/Suspended/Pending)
    /// Managed by administrators
    /// </summary>
    public DriverStatus Status { get; set; } = DriverStatus.Pending;

    /// <summary>
    /// Security flags for admin marking
    /// Comma-separated flags: "verified", "flagged", "needs_review"
    /// </summary>
    public string SecurityFlags { get; set; } = string.Empty;

    /// <summary>
    /// Accessibility features available in vehicle
    /// Comma-separated: "wheelchair_accessible", "service_animal_friendly", "hearing_assistance"
    /// </summary>
    public string AccessibilityFeatures { get; set; } = string.Empty;

    /// <summary>
    /// History of rides (JSON or comma-separated ride IDs)
    /// Alternative: use User.history field
    /// </summary>
    public string History { get; set; } = string.Empty;

    /// <summary>
    /// When driver profile was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    /// <summary>
    /// Associated user account
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Carpool offers created by this driver
    /// </summary>
    public ICollection<CarpoolOffer> CarpoolOffers { get; set; } = new List<CarpoolOffer>();
}
