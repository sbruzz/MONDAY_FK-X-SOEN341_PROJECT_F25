namespace CampusEvents.Services;

/// <summary>
/// Application-wide constants and configuration values.
/// Centralizes magic numbers and string literals for maintainability and consistency.
/// </summary>
/// <remarks>
/// This class provides a centralized location for all application constants,
/// making it easier to maintain and update values across the application.
/// Constants are organized into nested classes by category for better organization.
/// 
/// Benefits of using this class:
/// - Single source of truth for configuration values
/// - Easy to update values in one place
/// - Prevents magic numbers in code
/// - Improves code readability
/// - Facilitates testing and configuration changes
/// </remarks>
public static class Constants
{
    /// <summary>
    /// Session-related constants
    /// </summary>
    public static class Session
    {
        /// <summary>
        /// Session key for storing user ID
        /// </summary>
        public const string UserIdKey = "UserId";
        
        /// <summary>
        /// Session key for storing user role
        /// </summary>
        public const string UserRoleKey = "UserRole";
        
        /// <summary>
        /// Session key for storing user name
        /// </summary>
        public const string UserNameKey = "UserName";
        
        /// <summary>
        /// Default session timeout in minutes
        /// </summary>
        public const int DefaultTimeoutMinutes = 30;
    }

    /// <summary>
    /// Validation-related constants
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// Minimum length for user passwords
        /// </summary>
        public const int MinPasswordLength = 8;
        
        /// <summary>
        /// Maximum length for user passwords
        /// </summary>
        public const int MaxPasswordLength = 128;
        
        /// <summary>
        /// Minimum length for event titles
        /// </summary>
        public const int MinEventTitleLength = 3;
        
        /// <summary>
        /// Maximum length for event titles
        /// </summary>
        public const int MaxEventTitleLength = 200;
        
        /// <summary>
        /// Minimum length for event descriptions
        /// </summary>
        public const int MinEventDescriptionLength = 10;
        
        /// <summary>
        /// Maximum length for event descriptions
        /// </summary>
        public const int MaxEventDescriptionLength = 5000;
        
        /// <summary>
        /// Minimum capacity for events
        /// </summary>
        public const int MinEventCapacity = 1;
        
        /// <summary>
        /// Maximum capacity for events
        /// </summary>
        public const int MaxEventCapacity = 10000;
        
        /// <summary>
        /// Minimum capacity for carpool vehicles
        /// </summary>
        public const int MinCarpoolCapacity = 1;
        
        /// <summary>
        /// Maximum capacity for carpool vehicles
        /// </summary>
        public const int MaxCarpoolCapacity = 50;
        
        /// <summary>
        /// Minimum capacity for rooms
        /// </summary>
        public const int MinRoomCapacity = 1;
        
        /// <summary>
        /// Maximum capacity for rooms
        /// </summary>
        public const int MaxRoomCapacity = 1000;
    }

    /// <summary>
    /// Proximity and distance-related constants
    /// </summary>
    public static class Proximity
    {
        /// <summary>
        /// Earth's radius in kilometers (for Haversine formula)
        /// </summary>
        public const double EarthRadiusKm = 6371.0;
        
        /// <summary>
        /// Default proximity threshold in kilometers
        /// </summary>
        public const double DefaultThresholdKm = 10.0;
        
        /// <summary>
        /// Maximum proximity threshold in kilometers
        /// </summary>
        public const double MaxThresholdKm = 100.0;
        
        /// <summary>
        /// Average city driving speed in km/h (for travel time estimation)
        /// </summary>
        public const double AverageCitySpeedKmh = 40.0;
    }

    /// <summary>
    /// Pricing and currency-related constants
    /// </summary>
    public static class Pricing
    {
        /// <summary>
        /// Minimum price for paid tickets
        /// </summary>
        public const decimal MinTicketPrice = 0.01m;
        
        /// <summary>
        /// Maximum price for paid tickets
        /// </summary>
        public const decimal MaxTicketPrice = 10000.00m;
        
        /// <summary>
        /// Minimum hourly rate for room rentals
        /// </summary>
        public const decimal MinHourlyRate = 0.00m;
        
        /// <summary>
        /// Maximum hourly rate for room rentals
        /// </summary>
        public const decimal MaxHourlyRate = 1000.00m;
    }

    /// <summary>
    /// Date and time-related constants
    /// </summary>
    public static class DateTime
    {
        /// <summary>
        /// Default timezone identifier (Eastern Time)
        /// </summary>
        public const string DefaultTimeZone = "Eastern Standard Time";
        
        /// <summary>
        /// Minimum date for events (cannot create events in the past)
        /// </summary>
        public static System.DateTime MinEventDate => System.DateTime.UtcNow;
        
        /// <summary>
        /// Maximum date for events (reasonable future limit)
        /// </summary>
        public static System.DateTime MaxEventDate => System.DateTime.UtcNow.AddYears(5);
    }

    /// <summary>
    /// File and upload-related constants
    /// </summary>
    public static class FileUpload
    {
        /// <summary>
        /// Maximum file size for CSV uploads in bytes (10MB)
        /// </summary>
        public const long MaxCsvFileSize = 10 * 1024 * 1024;
        
        /// <summary>
        /// Maximum file size for QR code images in bytes (5MB)
        /// </summary>
        public const long MaxImageFileSize = 5 * 1024 * 1024;
        
        /// <summary>
        /// Allowed CSV file extensions
        /// </summary>
        public static readonly string[] AllowedCsvExtensions = { ".csv" };
        
        /// <summary>
        /// Allowed image file extensions
        /// </summary>
        public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    }

    /// <summary>
    /// Pagination-related constants
    /// </summary>
    public static class Pagination
    {
        /// <summary>
        /// Default page size for paginated results
        /// </summary>
        public const int DefaultPageSize = 20;
        
        /// <summary>
        /// Minimum page size
        /// </summary>
        public const int MinPageSize = 5;
        
        /// <summary>
        /// Maximum page size
        /// </summary>
        public const int MaxPageSize = 100;
    }

    /// <summary>
    /// Error message templates
    /// </summary>
    public static class ErrorMessages
    {
        /// <summary>
        /// Error message for invalid user credentials
        /// </summary>
        public const string InvalidCredentials = "Invalid email or password";
        
        /// <summary>
        /// Error message for unauthorized access
        /// </summary>
        public const string Unauthorized = "You are not authorized to perform this action";
        
        /// <summary>
        /// Error message for resource not found
        /// </summary>
        public const string NotFound = "The requested resource was not found";
        
        /// <summary>
        /// Error message for validation failure
        /// </summary>
        public const string ValidationFailed = "The provided data failed validation";
        
        /// <summary>
        /// Error message for capacity exceeded
        /// </summary>
        public const string CapacityExceeded = "The event has reached its maximum capacity";
    }

    /// <summary>
    /// Success message templates
    /// </summary>
    public static class SuccessMessages
    {
        /// <summary>
        /// Success message for successful login
        /// </summary>
        public const string LoginSuccess = "Successfully logged in";
        
        /// <summary>
        /// Success message for successful registration
        /// </summary>
        public const string RegistrationSuccess = "Account created successfully";
        
        /// <summary>
        /// Success message for successful ticket claim
        /// </summary>
        public const string TicketClaimed = "Ticket claimed successfully";
        
        /// <summary>
        /// Success message for successful event creation
        /// </summary>
        public const string EventCreated = "Event created successfully and is pending approval";
    }
}

