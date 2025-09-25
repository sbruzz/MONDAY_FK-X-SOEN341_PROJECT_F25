namespace CampusEvents.Services;

/// <summary>
/// Utility class for date and time operations
/// Provides helper methods for formatting, parsing, and manipulating dates
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Formats a DateTime for display in a user-friendly format
    /// </summary>
    /// <param name="dateTime">DateTime to format</param>
    /// <param name="includeTime">Whether to include time in the output</param>
    /// <returns>Formatted date string</returns>
    public static string FormatForDisplay(DateTime dateTime, bool includeTime = true)
    {
        if (includeTime)
        {
            return dateTime.ToString("MMMM dd, yyyy 'at' h:mm tt");
        }
        return dateTime.ToString("MMMM dd, yyyy");
    }

    /// <summary>
    /// Formats a DateTime for display with relative time (e.g., "2 hours ago")
    /// </summary>
    /// <param name="dateTime">DateTime to format</param>
    /// <returns>Relative time string</returns>
    public static string FormatRelativeTime(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalSeconds < 60)
            return "just now";

        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago";

        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago";

        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago";

        if (timeSpan.TotalDays < 30)
        {
            var weeks = (int)(timeSpan.TotalDays / 7);
            return $"{weeks} week{(weeks >= 2 ? "s" : "")} ago";
        }

        if (timeSpan.TotalDays < 365)
        {
            var months = (int)(timeSpan.TotalDays / 30);
            return $"{months} month{(months >= 2 ? "s" : "")} ago";
        }

        var years = (int)(timeSpan.TotalDays / 365);
        return $"{years} year{(years >= 2 ? "s" : "")} ago";
    }

    /// <summary>
    /// Calculates the duration between two dates in a human-readable format
    /// </summary>
    /// <param name="start">Start date</param>
    /// <param name="end">End date</param>
    /// <returns>Formatted duration string</returns>
    public static string FormatDuration(DateTime start, DateTime end)
    {
        var duration = end - start;

        if (duration.TotalDays >= 1)
            return $"{(int)duration.TotalDays} day{(duration.TotalDays >= 2 ? "s" : "")}";

        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours} hour{(duration.TotalHours >= 2 ? "s" : "")}";

        if (duration.TotalMinutes >= 1)
            return $"{(int)duration.TotalMinutes} minute{(duration.TotalMinutes >= 2 ? "s" : "")}";

        return $"{(int)duration.TotalSeconds} second{(duration.TotalSeconds >= 2 ? "s" : "")}";
    }

    /// <summary>
    /// Checks if a date is within a specified range
    /// </summary>
    /// <param name="date">Date to check</param>
    /// <param name="start">Start of range</param>
    /// <param name="end">End of range</param>
    /// <returns>True if date is within range</returns>
    public static bool IsWithinRange(DateTime date, DateTime start, DateTime end)
    {
        return date >= start && date <= end;
    }

    /// <summary>
    /// Gets the start of the day for a given date
    /// </summary>
    /// <param name="date">Date to get start of day for</param>
    /// <returns>DateTime at 00:00:00 of the given date</returns>
    public static DateTime StartOfDay(DateTime date)
    {
        return date.Date;
    }

    /// <summary>
    /// Gets the end of the day for a given date
    /// </summary>
    /// <param name="date">Date to get end of day for</param>
    /// <returns>DateTime at 23:59:59 of the given date</returns>
    public static DateTime EndOfDay(DateTime date)
    {
        return date.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Converts UTC time to local time for display
    /// </summary>
    /// <param name="utcTime">UTC DateTime</param>
    /// <param name="timeZoneId">Time zone identifier (default: Eastern Time)</param>
    /// <returns>Local DateTime</returns>
    public static DateTime ToLocalTime(DateTime utcTime, string timeZoneId = "Eastern Standard Time")
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
        }
        catch
        {
            // Fallback to system local time if timezone not found
            return utcTime.ToLocalTime();
        }
    }
}

