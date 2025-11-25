namespace CampusEvents.Models;

/// <summary>
/// Vehicle types available for carpooling.
/// Used to classify vehicles by size and passenger capacity.
/// </summary>
/// <remarks>
/// Vehicle types help passengers understand the type of vehicle they'll be riding in
/// and help the system estimate capacity and comfort levels.
/// 
/// Capacity Guidelines:
/// - Mini: 1-2 passengers (small car)
/// - Sedan: 3-4 passengers (standard car)
/// - SUV: 5-7 passengers (sport utility vehicle)
/// - Van: 8-12 passengers (large van)
/// - Bus: 13+ passengers (bus or large vehicle)
/// </remarks>
public enum VehicleType
{
    /// <summary>
    /// Mini vehicle - small car, typically 1-2 passengers
    /// </summary>
    Mini,
    
    /// <summary>
    /// Sedan - standard car, typically 3-4 passengers
    /// </summary>
    Sedan,
    
    /// <summary>
    /// SUV - sport utility vehicle, typically 5-7 passengers
    /// </summary>
    SUV,
    
    /// <summary>
    /// Van - large van, typically 8-12 passengers
    /// </summary>
    Van,
    
    /// <summary>
    /// Bus - bus or very large vehicle, typically 13+ passengers
    /// </summary>
    Bus
}

/// <summary>
/// Driver status managed by administrators.
/// Controls whether a driver can create carpool offers and participate in the carpool system.
/// </summary>
/// <remarks>
/// Driver status workflow:
/// - Pending: Initial state after registration, awaiting admin approval
/// - Active: Approved by admin, can create carpool offers
/// - Suspended: Temporarily disabled by admin (safety concerns, violations, etc.)
/// 
/// Status Changes:
/// - Pending → Active: Admin approval
/// - Active → Suspended: Admin suspension (with reason)
/// - Suspended → Active: Admin unsuspension
/// </remarks>
public enum DriverStatus
{
    /// <summary>
    /// Active driver - approved and can create carpool offers
    /// </summary>
    Active,
    
    /// <summary>
    /// Suspended driver - temporarily disabled by admin
    /// </summary>
    Suspended,
    
    /// <summary>
    /// Pending approval - awaiting admin review
    /// </summary>
    Pending
}

/// <summary>
/// Driver type indicating who can register as a driver.
/// Determines eligibility based on user role.
/// </summary>
/// <remarks>
/// Driver types enforce role-based eligibility:
/// - Student: Only users with Student role can register as student drivers
/// - Organizer: Only users with Organizer role can register as organizer drivers
/// 
/// This ensures that driver registrations match user roles and prevents
/// unauthorized driver registrations.
/// </remarks>
public enum DriverType
{
    /// <summary>
    /// Student driver - only students can register as this type
    /// </summary>
    Student,
    
    /// <summary>
    /// Organizer driver - only organizers can register as this type
    /// </summary>
    Organizer
}

/// <summary>
/// Driver entity for the carpool system (User Story US.04, Task.36).
/// Manages driver profiles with vehicle information, license details, and administrative controls.
/// </summary>
/// <remarks>
/// The Driver entity represents a user's driver profile in the carpool system.
/// It contains all information needed for drivers to offer rides and for administrators
/// to manage driver accounts.
/// 
/// Key Features:
/// - Driver registration with validation
/// - Vehicle information (type, capacity)
/// - License information (encrypted for security)
/// - Accessibility features
/// - Administrative controls (status, security flags)
/// - Support for multiple driver profiles per user (organizers)
/// 
/// Business Rules:
/// - Students can have at most one driver profile
/// - Organizers can have multiple driver profiles (different vehicles)
/// - Driver type must match user role (Student/Organizer)
/// - Capacity must be between 1 and 50 passengers
/// - Driver license number is encrypted at rest (AES-256)
/// - License plate is encrypted at rest (AES-256)
/// - Requires admin approval before creating offers
/// - Status changes are logged in SecurityFlags
/// 
/// Security:
/// - DriverLicenseNumber: Encrypted using AES-256 via EncryptionService
/// - LicensePlate: Encrypted using AES-256 via EncryptionService
/// - Encryption keys stored in configuration (environment variables in production)
/// - Only administrators can view decrypted license information
/// 
/// Status Management:
/// - Pending: New registration, awaiting admin approval
/// - Active: Approved, can create carpool offers
/// - Suspended: Disabled by admin (safety concerns, violations)
/// 
/// Security Flags:
/// - Comma-separated flags for admin marking
/// - Examples: "verified", "flagged", "suspended:2025-01-15:reason"
/// - Used for tracking driver history and admin actions
/// 
/// Accessibility Features:
/// - Comma-separated list of accessibility options
/// - Examples: "wheelchair_accessible", "service_animal_friendly", "hearing_assistance"
/// - Helps passengers find suitable rides
/// 
/// Relationships:
/// - Many-to-One: Driver → User (driver belongs to a user)
/// - One-to-Many: Driver → CarpoolOffer (driver creates offers)
/// 
/// Backward Compatibility:
/// This class includes several NotMapped properties for backward compatibility
/// with older UI code that may reference properties by different names or structures.
/// 
/// Example Usage:
/// ```csharp
/// // Register as driver
/// var driver = new Driver
/// {
///     UserId = userId,
///     Capacity = 4,
///     VehicleType = VehicleType.Sedan,
///     DriverType = DriverType.Student,
///     DriverLicenseNumber = _encryptionService.EncryptLicenseNumber("D12345678"),
///     Province = "QC",
///     LicensePlate = _encryptionService.EncryptLicensePlate("ABC-123"),
///     Status = DriverStatus.Pending, // Requires admin approval
///     AccessibilityFeatures = "wheelchair_accessible"
/// };
/// ```
/// </remarks>
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
