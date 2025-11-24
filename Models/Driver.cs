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
    /// Driver's license number (ENCRYPTED)
    /// Required for driver verification
    /// </summary>
    public string? DriverLicenseNumber { get; set; }

    /// <summary>
    /// Province/Territory code for license validation
    /// Two-letter code: ON, QC, BC, AB, SK, MB, NS, NB, PE, NL, YT, NT, NU
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// License plate number (ENCRYPTED)
    /// Optional vehicle identifier
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

    // Backward compatibility computed properties (not mapped to database)

    /// <summary>
    /// Alias for DriverType (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DriverType Type
    {
        get => DriverType;
        set => DriverType = value;
    }

    /// <summary>
    /// Computed property for backward compatibility
    /// Returns true if Status is Active
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsActive
    {
        get => Status == DriverStatus.Active;
        set => Status = value ? DriverStatus.Active : DriverStatus.Suspended;
    }

    /// <summary>
    /// Computed property for backward compatibility
    /// Returns true if SecurityFlags contains "flagged" or "suspended"
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsMarkedByAdmin
    {
        get => SecurityFlags.Contains("flagged") || SecurityFlags.Contains("suspended");
        set
        {
            if (value && !IsMarkedByAdmin)
            {
                SecurityFlags = string.IsNullOrEmpty(SecurityFlags) ? "flagged" : SecurityFlags + ",flagged";
            }
            else if (!value && IsMarkedByAdmin)
            {
                SecurityFlags = SecurityFlags.Replace(",flagged", "").Replace("flagged,", "").Replace("flagged", "")
                    .Replace(",suspended", "").Replace("suspended,", "").Replace("suspended", "");
            }
        }
    }

    /// <summary>
    /// Computed collection of all passengers from all carpool offers (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public ICollection<CarpoolPassenger> Passengers => CarpoolOffers.SelectMany(o => o.Passengers).ToList();

    /// <summary>
    /// Legacy EventId property (for backward compatibility)
    /// Returns the first active carpool offer's EventId if available
    /// Setting this is ignored in the new architecture
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int? EventId
    {
        get => CarpoolOffers.FirstOrDefault()?.EventId;
        set { /* Ignored - EventId is set on CarpoolOffer */ }
    }

    /// <summary>
    /// Legacy Event property (for backward compatibility)
    /// Returns the first active carpool offer's Event if available
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Event? Event => CarpoolOffers.FirstOrDefault()?.Event;

    /// <summary>
    /// Legacy OrganizerId property (not used in new architecture)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int? OrganizerId { get; set; }

    /// <summary>
    /// Legacy HasAccessibility property (for backward compatibility)
    /// Returns true if AccessibilityFeatures is not empty
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool HasAccessibility
    {
        get => !string.IsNullOrEmpty(AccessibilityFeatures);
        set
        {
            if (value && string.IsNullOrEmpty(AccessibilityFeatures))
            {
                AccessibilityFeatures = "wheelchair_accessible";
            }
            else if (!value)
            {
                AccessibilityFeatures = string.Empty;
            }
        }
    }

    /// <summary>
    /// Legacy VehicleDescription property (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? VehicleDescription { get; set; }

    /// <summary>
    /// Legacy ContactPhone property (for backward compatibility)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? ContactPhone { get; set; }
}
