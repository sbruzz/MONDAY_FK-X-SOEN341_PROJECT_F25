namespace CampusEvents.Services;

/// <summary>
/// Utility class for email validation and formatting.
/// Provides comprehensive email-related helper methods for validation, normalization,
/// extraction, masking, and domain checking.
/// </summary>
/// <remarks>
/// This class provides static utility methods for working with email addresses throughout
/// the application. All methods handle null/empty input gracefully and return appropriate
/// default values.
/// 
/// Key Features:
/// - Email format validation using RFC 5322 compliant regex
/// - Email normalization (lowercase, trim)
/// - Domain and username extraction
/// - Email masking for privacy
/// - Common provider detection
/// - Domain validation (basic format check)
/// 
/// Email Validation:
/// - Uses simplified RFC 5322 compliant regex pattern
/// - Case-insensitive matching
/// - Validates format: username@domain.tld
/// - Handles null/empty gracefully
/// 
/// Email Normalization:
/// - Converts to lowercase (case-insensitive)
/// - Trims whitespace
/// - Ensures consistent email storage
/// 
/// Email Extraction:
/// - ExtractDomain: Gets domain part (after @)
/// - ExtractUsername: Gets username part (before @)
/// - Handles invalid formats gracefully
/// 
/// Email Masking:
/// - Masks username for privacy (e.g., "user@example.com" -> "u***@example.com")
/// - Preserves first character of username
/// - Keeps domain visible
/// - Useful for displaying emails in logs/UI
/// 
/// Common Provider Detection:
/// - Checks if email is from common providers (Gmail, Yahoo, etc.)
/// - Useful for user experience features
/// - Can be used for provider-specific handling
/// 
/// Domain Validation:
/// - Basic domain format validation
/// - Placeholder for full DNS MX record lookup
/// - Validates domain structure
/// - Does not verify domain existence (would require DNS lookup)
/// 
/// Example Usage:
/// ```csharp
/// // Validate email
/// if (!EmailHelper.IsValidEmail(email))
///     return (false, "Invalid email format", null);
/// 
/// // Normalize email before storage
/// var normalized = EmailHelper.NormalizeEmail(userInput);
/// user.Email = normalized;
/// 
/// // Extract domain for analysis
/// var domain = EmailHelper.ExtractDomain(email);
/// if (domain == "concordia.ca")
///     // Handle Concordia email
/// 
/// // Mask email for display
/// var masked = EmailHelper.MaskEmail(user.Email);
/// // Display: "u***@example.com"
/// 
/// // Check if common provider
/// if (EmailHelper.IsCommonProvider(email))
///     // Show provider-specific features
/// ```
/// 
/// Security Considerations:
/// - Always normalize emails before storage
/// - Validate email format before processing
/// - Mask emails in logs/public displays
/// - Consider rate limiting for email validation
/// </remarks>
public static class EmailHelper
{
    /// <summary>
    /// Validates email format using comprehensive regex pattern
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if email format is valid</returns>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // RFC 5322 compliant email regex (simplified)
            var emailRegex = new System.Text.RegularExpressions.Regex(
                @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            return emailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Normalizes email address (lowercase, trim whitespace)
    /// </summary>
    /// <param name="email">Email address to normalize</param>
    /// <returns>Normalized email address</returns>
    public static string NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        return email.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Extracts domain from email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Domain part of email, or empty string if invalid</returns>
    public static string ExtractDomain(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        var normalized = NormalizeEmail(email);
        var atIndex = normalized.IndexOf('@');
        
        if (atIndex < 0 || atIndex >= normalized.Length - 1)
            return string.Empty;

        return normalized.Substring(atIndex + 1);
    }

    /// <summary>
    /// Extracts username from email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Username part of email, or empty string if invalid</returns>
    public static string ExtractUsername(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        var normalized = NormalizeEmail(email);
        var atIndex = normalized.IndexOf('@');
        
        if (atIndex <= 0)
            return string.Empty;

        return normalized.Substring(0, atIndex);
    }

    /// <summary>
    /// Masks email address for privacy (e.g., "user@example.com" -> "u***@example.com")
    /// </summary>
    /// <param name="email">Email address to mask</param>
    /// <returns>Masked email address</returns>
    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        var normalized = NormalizeEmail(email);
        var atIndex = normalized.IndexOf('@');
        
        if (atIndex <= 0)
            return normalized;

        var username = normalized.Substring(0, atIndex);
        var domain = normalized.Substring(atIndex + 1);

        if (username.Length <= 1)
            return $"{username[0]}***@{domain}";

        var maskedUsername = $"{username[0]}{new string('*', username.Length - 1)}";
        return $"{maskedUsername}@{domain}";
    }

    /// <summary>
    /// Checks if email is from a common email provider
    /// </summary>
    /// <param name="email">Email address to check</param>
    /// <returns>True if email is from a common provider</returns>
    public static bool IsCommonProvider(string? email)
    {
        var domain = ExtractDomain(email);
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        var commonProviders = new[]
        {
            "gmail.com", "yahoo.com", "hotmail.com", "outlook.com",
            "icloud.com", "aol.com", "mail.com", "protonmail.com"
        };

        return commonProviders.Contains(domain);
    }

    /// <summary>
    /// Validates email domain exists (basic DNS check)
    /// Note: This is a placeholder - full implementation would require DNS lookup
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if domain appears valid (placeholder implementation)</returns>
    public static bool ValidateDomain(string? email)
    {
        var domain = ExtractDomain(email);
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        // Basic domain format validation
        // Full implementation would perform DNS MX record lookup
        return System.Text.RegularExpressions.Regex.IsMatch(
            domain,
            @"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$");
    }
}

