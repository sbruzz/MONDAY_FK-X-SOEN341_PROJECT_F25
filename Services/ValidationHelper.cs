using CampusEvents.Models;

namespace CampusEvents.Services;

/// <summary>
/// Utility class for common validation operations across the application.
/// Provides comprehensive, reusable validation methods for user inputs, business rules,
/// and data integrity checks.
/// </summary>
/// <remarks>
/// This class provides static utility methods for validating various types of input
/// and data throughout the application. All validation methods are designed to be
/// consistent, reusable, and provide clear validation results.
/// 
/// Key Features:
/// - Email format validation
/// - Student ID validation (9-digit format)
/// - String length validation
/// - Date range validation
/// - Future date validation
/// - Capacity validation with bounds
/// - Geographic coordinate validation (latitude/longitude)
/// - Input sanitization (XSS prevention)
/// 
/// Validation Categories:
/// 
/// Email Validation:
/// - Basic regex pattern matching
/// - Case-insensitive
/// - Handles null/empty gracefully
/// 
/// Student ID Validation:
/// - 9-digit format (Concordia student IDs)
/// - Numeric only
/// - Exact length validation
/// 
/// String Validation:
/// - Null/empty/whitespace checking
/// - Minimum length validation
/// - Maximum length validation (optional)
/// 
/// Date Validation:
/// - Date range validation (end after start)
/// - Future date validation
/// - UTC timezone handling
/// 
/// Capacity Validation:
/// - Positive number validation
/// - Minimum/maximum bounds
/// - Configurable limits
/// 
/// Coordinate Validation:
/// - Latitude: -90 to 90 degrees
/// - Longitude: -180 to 180 degrees
/// - Both coordinates validated together
/// 
/// Input Sanitization:
/// - Removes HTML tags (XSS prevention)
/// - Removes control characters
/// - Trims whitespace
/// - Returns safe string for display
/// 
/// Security Considerations:
/// - SanitizeInput removes potentially dangerous characters
/// - Prevents XSS attacks through HTML tag removal
/// - Validates input before processing
/// - Never trusts user input
/// 
/// Example Usage:
/// ```csharp
/// // Email validation
/// if (!ValidationHelper.IsValidEmail(email))
///     return (false, "Invalid email format", null);
/// 
/// // Student ID validation
/// if (!ValidationHelper.IsValidStudentId(studentId))
///     return (false, "Invalid student ID format", null);
/// 
/// // String validation
/// if (!ValidationHelper.IsValidString(title, minLength: 3, maxLength: 200))
///     return (false, "Invalid title length", null);
/// 
/// // Date validation
/// if (!ValidationHelper.IsFutureDate(eventDate))
///     return (false, "Event date must be in the future", null);
/// 
/// // Capacity validation
/// if (!ValidationHelper.IsValidCapacity(capacity, min: 1, max: 100))
///     return (false, "Invalid capacity", null);
/// 
/// // Coordinate validation
/// if (!ValidationHelper.IsValidCoordinates(latitude, longitude))
///     return (false, "Invalid coordinates", null);
/// 
/// // Input sanitization
/// var sanitized = ValidationHelper.SanitizeInput(userInput);
/// ```
/// 
/// Best Practices:
/// - Always validate input before processing
/// - Use appropriate validation methods for each data type
/// - Sanitize user input before display
/// - Provide clear error messages
/// - Validate on both client and server side
/// </remarks>
public static class ValidationHelper
{
    /// <summary>
    /// Validates email format using basic regex pattern
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if email format is valid, false otherwise</returns>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var emailRegex = new System.Text.RegularExpressions.Regex(
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates student ID format (9 digits)
    /// </summary>
    /// <param name="studentId">Student ID to validate</param>
    /// <returns>True if student ID format is valid</returns>
    public static bool IsValidStudentId(string? studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(studentId, @"^\d{9}$");
    }

    /// <summary>
    /// Validates that a string is not null, empty, or whitespace
    /// </summary>
    /// <param name="value">String to validate</param>
    /// <param name="minLength">Minimum required length (default: 1)</param>
    /// <param name="maxLength">Maximum allowed length (default: no limit)</param>
    /// <returns>True if string is valid</returns>
    public static bool IsValidString(string? value, int minLength = 1, int? maxLength = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.Length < minLength)
            return false;

        if (maxLength.HasValue && value.Length > maxLength.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Validates date range (end date must be after start date)
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>True if date range is valid</returns>
    public static bool IsValidDateRange(DateTime startDate, DateTime endDate)
    {
        return endDate > startDate;
    }

    /// <summary>
    /// Validates that a date is in the future
    /// </summary>
    /// <param name="date">Date to validate</param>
    /// <returns>True if date is in the future</returns>
    public static bool IsFutureDate(DateTime date)
    {
        return date > DateTime.UtcNow;
    }

    /// <summary>
    /// Validates capacity value (must be positive and within reasonable bounds)
    /// </summary>
    /// <param name="capacity">Capacity value to validate</param>
    /// <param name="min">Minimum allowed capacity (default: 1)</param>
    /// <param name="max">Maximum allowed capacity (default: 1000)</param>
    /// <returns>True if capacity is valid</returns>
    public static bool IsValidCapacity(int capacity, int min = 1, int max = 1000)
    {
        return capacity >= min && capacity <= max;
    }

    /// <summary>
    /// Validates latitude value (must be between -90 and 90)
    /// </summary>
    /// <param name="latitude">Latitude to validate</param>
    /// <returns>True if latitude is valid</returns>
    public static bool IsValidLatitude(double latitude)
    {
        return latitude >= -90.0 && latitude <= 90.0;
    }

    /// <summary>
    /// Validates longitude value (must be between -180 and 180)
    /// </summary>
    /// <param name="longitude">Longitude to validate</param>
    /// <returns>True if longitude is valid</returns>
    public static bool IsValidLongitude(double longitude)
    {
        return longitude >= -180.0 && longitude <= 180.0;
    }

    /// <summary>
    /// Validates coordinates (both latitude and longitude)
    /// </summary>
    /// <param name="latitude">Latitude to validate</param>
    /// <param name="longitude">Longitude to validate</param>
    /// <returns>True if both coordinates are valid</returns>
    public static bool IsValidCoordinates(double? latitude, double? longitude)
    {
        if (!latitude.HasValue || !longitude.HasValue)
            return false;

        return IsValidLatitude(latitude.Value) && IsValidLongitude(longitude.Value);
    }

    /// <summary>
    /// Sanitizes user input by removing potentially dangerous characters
    /// </summary>
    /// <param name="input">Input string to sanitize</param>
    /// <returns>Sanitized string</returns>
    public static string SanitizeInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove HTML tags and script content
        var sanitized = System.Text.RegularExpressions.Regex.Replace(
            input,
            @"<[^>]*>",
            string.Empty);

        // Remove control characters
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"[\x00-\x1F\x7F]",
            string.Empty);

        return sanitized.Trim();
    }
}

