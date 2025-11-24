namespace CampusEvents.Services;

/// <summary>
/// Utility class for email validation and formatting
/// Provides email-related helper methods
/// </summary>
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

