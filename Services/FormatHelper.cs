namespace CampusEvents.Services;

/// <summary>
/// Utility class for formatting various data types for display.
/// Provides consistent formatting across the application for currency, percentages,
/// file sizes, phone numbers, student IDs, capacity, lists, ordinals, and time spans.
/// </summary>
/// <remarks>
/// This class provides static utility methods for formatting data in a user-friendly
/// and consistent manner throughout the application. All formatting methods handle
/// null/empty values gracefully and return appropriate default values.
/// 
/// Key Features:
/// - Currency formatting with customizable currency symbol
/// - Percentage formatting with configurable decimal places
/// - File size formatting (B, KB, MB, GB, TB)
/// - Phone number formatting (North American format)
/// - Student ID formatting (9-digit format)
/// - Capacity formatting (current/max and percentage)
/// - List formatting with truncation support
/// - Ordinal number formatting (1st, 2nd, 3rd, etc.)
/// - Time span formatting (human-readable)
/// 
/// Formatting Standards:
/// - Currency: $XX.XX format (2 decimal places)
/// - Percentage: XX.X% format (configurable decimals)
/// - File Size: Human-readable with appropriate unit (B, KB, MB, GB, TB)
/// - Phone: (XXX) XXX-XXXX format (North American)
/// - Student ID: XXX-XXX-XXX format (9 digits)
/// - Capacity: "XX / YY" or "XX.X%" format
/// 
/// Null Handling:
/// - All methods handle null/empty input gracefully
/// - Return empty string or default value for null input
/// - No exceptions thrown for invalid input
/// 
/// Example Usage:
/// ```csharp
/// // Currency
/// var price = FormatHelper.FormatCurrency(25.50m); // "$25.50"
/// 
/// // Percentage
/// var percent = FormatHelper.FormatPercentage(0.75, decimals: 1); // "75.0%"
/// 
/// // File size
/// var size = FormatHelper.FormatFileSize(1048576); // "1 MB"
/// 
/// // Phone number
/// var phone = FormatHelper.FormatPhoneNumber("5141234567"); // "(514) 123-4567"
/// 
/// // Student ID
/// var studentId = FormatHelper.FormatStudentId("40294756"); // "402-947-56"
/// 
/// // Capacity
/// var capacity = FormatHelper.FormatCapacity(50, 100); // "50 / 100"
/// var capacityPercent = FormatHelper.FormatCapacityPercentage(50, 100); // "50.0%"
/// 
/// // Ordinal
/// var ordinal = FormatHelper.FormatOrdinal(1); // "1st"
/// var ordinal2 = FormatHelper.FormatOrdinal(22); // "22nd"
/// 
/// // Time span
/// var duration = FormatHelper.FormatTimeSpan(TimeSpan.FromHours(2)); // "2 hours"
/// ```
/// </remarks>
public static class FormatHelper
{
    /// <summary>
    /// Formats currency amount for display
    /// </summary>
    /// <param name="amount">Amount to format</param>
    /// <param name="currency">Currency symbol (default: $)</param>
    /// <returns>Formatted currency string</returns>
    public static string FormatCurrency(decimal amount, string currency = "$")
    {
        return $"{currency}{amount:F2}";
    }

    /// <summary>
    /// Formats percentage for display
    /// </summary>
    /// <param name="value">Value to format (0.0 to 1.0)</param>
    /// <param name="decimals">Number of decimal places</param>
    /// <returns>Formatted percentage string</returns>
    public static string FormatPercentage(double value, int decimals = 1)
    {
        return string.Format($"{{0:F{decimals}}}%", value * 100);
    }

    /// <summary>
    /// Formats file size in human-readable format
    /// </summary>
    /// <param name="bytes">File size in bytes</param>
    /// <returns>Formatted file size string</returns>
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Formats phone number for display
    /// </summary>
    /// <param name="phoneNumber">Phone number to format</param>
    /// <returns>Formatted phone number string</returns>
    public static string FormatPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        // Remove all non-digit characters
        var digits = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"\D", "");

        // Format based on length
        if (digits.Length == 10)
        {
            // North American format: (123) 456-7890
            return $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 4)}";
        }
        else if (digits.Length == 11 && digits[0] == '1')
        {
            // North American format with country code: +1 (123) 456-7890
            return $"+1 ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits.Substring(7, 4)}";
        }

        // Return original if format doesn't match
        return phoneNumber;
    }

    /// <summary>
    /// Formats student ID for display
    /// </summary>
    /// <param name="studentId">Student ID to format</param>
    /// <returns>Formatted student ID string</returns>
    public static string FormatStudentId(string? studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return string.Empty;

        // Remove all non-digit characters
        var digits = System.Text.RegularExpressions.Regex.Replace(studentId, @"\D", "");

        // Format as 9-digit ID: 123-456-789
        if (digits.Length == 9)
        {
            return $"{digits.Substring(0, 3)}-{digits.Substring(3, 3)}-{digits.Substring(6, 3)}";
        }

        return studentId;
    }

    /// <summary>
    /// Formats capacity information (e.g., "50 / 100")
    /// </summary>
    /// <param name="current">Current capacity used</param>
    /// <param name="max">Maximum capacity</param>
    /// <returns>Formatted capacity string</returns>
    public static string FormatCapacity(int current, int max)
    {
        return $"{current} / {max}";
    }

    /// <summary>
    /// Formats capacity as percentage
    /// </summary>
    /// <param name="current">Current capacity used</param>
    /// <param name="max">Maximum capacity</param>
    /// <returns>Formatted capacity percentage string</returns>
    public static string FormatCapacityPercentage(int current, int max)
    {
        if (max == 0)
            return "0%";

        var percentage = (double)current / max * 100;
        return FormatPercentage(percentage / 100);
    }

    /// <summary>
    /// Formats a list of items as a comma-separated string
    /// </summary>
    /// <param name="items">Items to format</param>
    /// <param name="maxItems">Maximum number of items to show (default: all)</param>
    /// <returns>Formatted string</returns>
    public static string FormatList(IEnumerable<string> items, int? maxItems = null)
    {
        var itemList = items?.Where(i => !string.IsNullOrWhiteSpace(i)).ToList() ?? new List<string>();

        if (itemList.Count == 0)
            return string.Empty;

        if (maxItems.HasValue && itemList.Count > maxItems.Value)
        {
            var shown = itemList.Take(maxItems.Value);
            var remaining = itemList.Count - maxItems.Value;
            return $"{string.Join(", ", shown)} and {remaining} more";
        }

        return string.Join(", ", itemList);
    }

    /// <summary>
    /// Formats a number with appropriate suffix (1st, 2nd, 3rd, etc.)
    /// </summary>
    /// <param name="number">Number to format</param>
    /// <returns>Formatted ordinal number string</returns>
    public static string FormatOrdinal(int number)
    {
        if (number <= 0)
            return number.ToString();

        int lastDigit = number % 10;
        int lastTwoDigits = number % 100;

        string suffix = lastTwoDigits switch
        {
            11 or 12 or 13 => "th",
            _ => lastDigit switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            }
        };

        return $"{number}{suffix}";
    }

    /// <summary>
    /// Formats a time span in human-readable format
    /// </summary>
    /// <param name="timeSpan">Time span to format</param>
    /// <returns>Formatted time span string</returns>
    public static string FormatTimeSpan(System.TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")}";

        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")}";

        if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")}";

        return $"{(int)timeSpan.TotalSeconds} second{(timeSpan.TotalSeconds >= 2 ? "s" : "")}";
    }
}

