namespace CampusEvents.Services;

/// <summary>
/// Utility class for application logging operations.
/// Provides structured logging helpers for consistent log formatting across the application.
/// </summary>
/// <remarks>
/// This class provides static methods for logging at different levels (Info, Warning, Error, Debug)
/// with consistent formatting that includes timestamps, log levels, and optional context information.
/// 
/// Log Format:
/// [YYYY-MM-DD HH:MM:SS] LEVEL [Context]: Message
/// 
/// Key Features:
/// - Structured log formatting with timestamps
/// - Multiple log levels (Info, Warning, Error, Debug)
/// - Context information for better traceability
/// - Method entry/exit logging for debugging
/// - Performance metrics logging
/// - Debug logging only in DEBUG builds
/// 
/// Important Notes:
/// - This is a simple logging helper that writes to console
/// - For production, consider using a proper logging framework (Serilog, NLog, etc.)
/// - Debug messages are only logged in DEBUG builds (compiled out in Release)
/// - Error messages are written to Console.Error, others to Console.Out
/// 
/// Example usage:
/// ```csharp
/// LoggingHelper.LogInfo("User logged in", "Authentication");
/// LoggingHelper.LogWarning("Invalid login attempt", "Security");
/// LoggingHelper.LogError("Database connection failed", "DataAccess");
/// LoggingHelper.LogDebug("Processing request", "RequestHandler");
/// LoggingHelper.LogPerformance("DatabaseQuery", TimeSpan.FromMilliseconds(150));
/// ```
/// 
/// For production applications, consider replacing this with:
/// - Serilog with file/console sinks
/// - Application Insights for cloud deployments
/// - ELK stack for centralized logging
/// </remarks>
public static class LoggingHelper
{
    /// <summary>
    /// Logs an information message
    /// </summary>
    /// <param name="message">Message to log</param>
    /// <param name="context">Context where logging occurred</param>
    public static void LogInfo(string message, string? context = null)
    {
        var logMessage = FormatLogMessage("INFO", message, context);
        Console.WriteLine(logMessage);
    }

    /// <summary>
    /// Logs a warning message
    /// </summary>
    /// <param name="message">Warning message</param>
    /// <param name="context">Context where warning occurred</param>
    public static void LogWarning(string message, string? context = null)
    {
        var logMessage = FormatLogMessage("WARN", message, context);
        Console.WriteLine(logMessage);
    }

    /// <summary>
    /// Logs an error message
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="context">Context where error occurred</param>
    public static void LogError(string message, string? context = null)
    {
        var logMessage = FormatLogMessage("ERROR", message, context);
        Console.Error.WriteLine(logMessage);
    }

    /// <summary>
    /// Logs a debug message (only in debug builds)
    /// </summary>
    /// <param name="message">Debug message</param>
    /// <param name="context">Context where debug occurred</param>
    public static void LogDebug(string message, string? context = null)
    {
        #if DEBUG
        var logMessage = FormatLogMessage("DEBUG", message, context);
        Console.WriteLine(logMessage);
        #endif
    }

    /// <summary>
    /// Formats a log message with timestamp and context
    /// </summary>
    /// <param name="level">Log level (INFO, WARN, ERROR, DEBUG)</param>
    /// <param name="message">Log message</param>
    /// <param name="context">Optional context</param>
    /// <returns>Formatted log message</returns>
    private static string FormatLogMessage(string level, string message, string? context)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var contextPart = string.IsNullOrWhiteSpace(context) ? "" : $" [{context}]";
        return $"[{timestamp}] {level}{contextPart}: {message}";
    }

    /// <summary>
    /// Logs method entry
    /// </summary>
    /// <param name="methodName">Name of the method</param>
    /// <param name="parameters">Method parameters (optional)</param>
    public static void LogMethodEntry(string methodName, string? parameters = null)
    {
        var message = $"Entering {methodName}";
        if (!string.IsNullOrWhiteSpace(parameters))
        {
            message += $" with parameters: {parameters}";
        }
        LogDebug(message, "MethodEntry");
    }

    /// <summary>
    /// Logs method exit
    /// </summary>
    /// <param name="methodName">Name of the method</param>
    /// <param name="result">Method result (optional)</param>
    public static void LogMethodExit(string methodName, string? result = null)
    {
        var message = $"Exiting {methodName}";
        if (!string.IsNullOrWhiteSpace(result))
        {
            message += $" with result: {result}";
        }
        LogDebug(message, "MethodExit");
    }

    /// <summary>
    /// Logs performance metrics
    /// </summary>
    /// <param name="operation">Operation name</param>
    /// <param name="duration">Duration of operation</param>
    public static void LogPerformance(string operation, TimeSpan duration)
    {
        var message = $"{operation} completed in {duration.TotalMilliseconds:F2}ms";
        LogInfo(message, "Performance");
    }
}

