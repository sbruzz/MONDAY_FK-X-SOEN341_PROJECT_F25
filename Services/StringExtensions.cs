namespace CampusEvents.Services;

/// <summary>
/// Extension methods for string operations.
/// Provides convenient string manipulation utilities that extend the built-in string class.
/// </summary>
/// <remarks>
/// This class contains extension methods that add functionality to the string type,
/// making common string operations more convenient and readable throughout the application.
/// 
/// Extension methods allow you to call these methods as if they were instance methods
/// on the string class, providing a more fluent API.
/// 
/// Example usage:
/// ```csharp
/// string text = "Hello World";
/// string truncated = text.Truncate(5); // Returns "He..."
/// string title = text.ToTitleCase(); // Returns "Hello World"
/// ```
/// 
/// All methods handle null and empty strings gracefully, returning appropriate
/// default values rather than throwing exceptions.
/// </remarks>
public static class StringExtensions
{
    /// <summary>
    /// Truncates a string to a specified maximum length
    /// </summary>
    /// <param name="value">String to truncate</param>
    /// <param name="maxLength">Maximum length</param>
    /// <param name="suffix">Suffix to append if truncated (default: "...")</param>
    /// <returns>Truncated string</returns>
    public static string Truncate(this string? value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// Capitalizes the first letter of a string
    /// </summary>
    /// <param name="value">String to capitalize</param>
    /// <returns>String with first letter capitalized</returns>
    public static string CapitalizeFirst(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Length == 1)
            return value.ToUpper();

        return char.ToUpper(value[0]) + value.Substring(1).ToLower();
    }

    /// <summary>
    /// Converts a string to title case (each word capitalized)
    /// </summary>
    /// <param name="value">String to convert</param>
    /// <returns>Title case string</returns>
    public static string ToTitleCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleCaseWords = words.Select(word => word.CapitalizeFirst());
        return string.Join(" ", titleCaseWords);
    }

    /// <summary>
    /// Removes extra whitespace from a string
    /// </summary>
    /// <param name="value">String to clean</param>
    /// <returns>String with normalized whitespace</returns>
    public static string NormalizeWhitespace(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return System.Text.RegularExpressions.Regex.Replace(
            value.Trim(),
            @"\s+",
            " ");
    }

    /// <summary>
    /// Checks if a string contains any of the specified substrings (case-insensitive)
    /// </summary>
    /// <param name="value">String to search in</param>
    /// <param name="substrings">Substrings to search for</param>
    /// <returns>True if any substring is found</returns>
    public static bool ContainsAny(this string? value, params string[] substrings)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return substrings.Any(substring =>
            value.Contains(substring, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Extracts a safe filename from a string
    /// </summary>
    /// <param name="value">String to convert to filename</param>
    /// <returns>Safe filename string</returns>
    public static string ToSafeFileName(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "file";

        var invalidChars = System.IO.Path.GetInvalidFileNameChars();
        var safeName = new string(value
            .Where(c => !invalidChars.Contains(c))
            .ToArray());

        return safeName.Truncate(255, "");
    }
}

