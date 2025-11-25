namespace CampusEvents.Services;

/// <summary>
/// Utility class for number operations and formatting.
/// Provides number manipulation, validation, and formatting helpers for numeric types.
/// </summary>
/// <remarks>
/// This class provides static utility methods for working with numeric values,
/// including formatting, parsing, validation, and mathematical operations.
/// 
/// Key Features:
/// - Number clamping and range validation
/// - Number formatting with separators and decimal places
/// - Safe parsing with default values
/// - Percentage calculations
/// - Rounding operations (round, ceiling, floor)
/// 
/// All parsing methods provide safe fallback values to prevent exceptions
/// when parsing invalid input. This makes the code more robust and user-friendly.
/// 
/// Example usage:
/// ```csharp
/// int clamped = NumberHelper.Clamp(value, 0, 100);
/// string formatted = NumberHelper.FormatWithSeparators(1234567); // "1,234,567"
/// int parsed = NumberHelper.ParseInt("123", defaultValue: 0);
/// ```
/// </remarks>
public static class NumberHelper
{
    /// <summary>
    /// Clamps a number between minimum and maximum values
    /// </summary>
    /// <typeparam name="T">Numeric type</typeparam>
    /// <param name="value">Value to clamp</param>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    /// <returns>Clamped value</returns>
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
            return min;
        if (value.CompareTo(max) > 0)
            return max;
        return value;
    }

    /// <summary>
    /// Checks if a number is within a range (inclusive)
    /// </summary>
    /// <typeparam name="T">Numeric type</typeparam>
    /// <param name="value">Value to check</param>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    /// <returns>True if value is within range</returns>
    public static bool IsInRange<T>(T value, T min, T max) where T : IComparable<T>
    {
        return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
    }

    /// <summary>
    /// Formats a number with thousand separators
    /// </summary>
    /// <param name="number">Number to format</param>
    /// <returns>Formatted number string</returns>
    public static string FormatWithSeparators(long number)
    {
        return number.ToString("N0");
    }

    /// <summary>
    /// Formats a decimal number with specified decimal places
    /// </summary>
    /// <param name="number">Decimal number to format</param>
    /// <param name="decimalPlaces">Number of decimal places</param>
    /// <returns>Formatted decimal string</returns>
    public static string FormatDecimal(decimal number, int decimalPlaces = 2)
    {
        return number.ToString($"F{decimalPlaces}");
    }

    /// <summary>
    /// Parses a string to integer with fallback
    /// </summary>
    /// <param name="value">String to parse</param>
    /// <param name="defaultValue">Default value if parsing fails</param>
    /// <returns>Parsed integer or default value</returns>
    public static int ParseInt(string? value, int defaultValue = 0)
    {
        if (int.TryParse(value, out int result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Parses a string to decimal with fallback
    /// </summary>
    /// <param name="value">String to parse</param>
    /// <param name="defaultValue">Default value if parsing fails</param>
    /// <returns>Parsed decimal or default value</returns>
    public static decimal ParseDecimal(string? value, decimal defaultValue = 0m)
    {
        if (decimal.TryParse(value, out decimal result))
            return result;
        return defaultValue;
    }

    /// <summary>
    /// Calculates percentage of a value
    /// </summary>
    /// <param name="value">Value to calculate percentage of</param>
    /// <param name="total">Total value</param>
    /// <returns>Percentage (0-100)</returns>
    public static double CalculatePercentage(double value, double total)
    {
        if (total == 0)
            return 0;
        return (value / total) * 100;
    }

    /// <summary>
    /// Rounds a number to specified number of decimal places
    /// </summary>
    /// <param name="value">Value to round</param>
    /// <param name="decimalPlaces">Number of decimal places</param>
    /// <returns>Rounded value</returns>
    public static double Round(double value, int decimalPlaces = 2)
    {
        return Math.Round(value, decimalPlaces);
    }

    /// <summary>
    /// Rounds up to the nearest integer
    /// </summary>
    /// <param name="value">Value to round up</param>
    /// <returns>Ceiling value</returns>
    public static int Ceiling(double value)
    {
        return (int)Math.Ceiling(value);
    }

    /// <summary>
    /// Rounds down to the nearest integer
    /// </summary>
    /// <param name="value">Value to round down</param>
    /// <returns>Floor value</returns>
    public static int Floor(double value)
    {
        return (int)Math.Floor(value);
    }
}

