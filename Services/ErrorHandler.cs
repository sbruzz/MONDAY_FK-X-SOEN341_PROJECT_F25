namespace CampusEvents.Services;

/// <summary>
/// Centralized error handling utility
/// Provides consistent error message formatting and logging
/// </summary>
public static class ErrorHandler
{
    /// <summary>
    /// Formats a user-friendly error message
    /// </summary>
    /// <param name="operation">Operation that failed</param>
    /// <param name="details">Additional error details</param>
    /// <returns>Formatted error message</returns>
    public static string FormatErrorMessage(string operation, string? details = null)
    {
        var message = $"An error occurred while {operation}.";
        
        if (!string.IsNullOrWhiteSpace(details))
        {
            message += $" {details}";
        }

        return message;
    }

    /// <summary>
    /// Logs an error with context information
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="context">Context where error occurred</param>
    /// <param name="additionalInfo">Additional information</param>
    public static void LogError(Exception exception, string context, string? additionalInfo = null)
    {
        // In production, this would use a proper logging framework (e.g., Serilog, NLog)
        var errorMessage = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ERROR in {context}";
        
        if (!string.IsNullOrWhiteSpace(additionalInfo))
        {
            errorMessage += $": {additionalInfo}";
        }

        errorMessage += $"\nException: {exception.GetType().Name}";
        errorMessage += $"\nMessage: {exception.Message}";
        
        if (exception.StackTrace != null)
        {
            errorMessage += $"\nStack Trace: {exception.StackTrace}";
        }

        // For now, write to console (in production, use proper logging)
        Console.Error.WriteLine(errorMessage);
    }

    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="code">Error code (optional)</param>
    /// <returns>Error response tuple</returns>
    public static (bool Success, string Message) CreateErrorResponse(string message, string? code = null)
    {
        var fullMessage = message;
        if (!string.IsNullOrWhiteSpace(code))
        {
            fullMessage = $"[{code}] {message}";
        }

        return (false, fullMessage);
    }

    /// <summary>
    /// Creates a standardized success response
    /// </summary>
    /// <param name="message">Success message</param>
    /// <returns>Success response tuple</returns>
    public static (bool Success, string Message) CreateSuccessResponse(string message)
    {
        return (true, message);
    }

    /// <summary>
    /// Validates and wraps an operation with error handling
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="errorMessage">Error message if operation fails</param>
    /// <returns>Result of operation or default value on error</returns>
    public static T? SafeExecute<T>(Func<T> operation, string errorMessage) where T : class
    {
        try
        {
            return operation();
        }
        catch (Exception ex)
        {
            LogError(ex, errorMessage);
            return default(T);
        }
    }

    /// <summary>
    /// Validates and wraps an async operation with error handling
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Async operation to execute</param>
    /// <param name="errorMessage">Error message if operation fails</param>
    /// <returns>Result of operation or default value on error</returns>
    public static async Task<T?> SafeExecuteAsync<T>(Func<Task<T>> operation, string errorMessage) where T : class
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            LogError(ex, errorMessage);
            return default(T);
        }
    }
}

