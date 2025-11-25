namespace CampusEvents.Services;

/// <summary>
/// Centralized error handling utility.
/// Provides consistent error message formatting, logging, and safe execution wrappers
/// for operations throughout the application.
/// </summary>
/// <remarks>
/// This class provides static utility methods for handling errors consistently across
/// the application. It ensures that errors are logged properly, formatted consistently,
/// and handled gracefully without exposing sensitive information to users.
/// 
/// Key Features:
/// - User-friendly error message formatting
/// - Comprehensive error logging with context
/// - Standardized error/success response creation
/// - Safe execution wrappers (sync and async)
/// - Exception handling and logging
/// 
/// Error Message Formatting:
/// - FormatErrorMessage: Creates user-friendly error messages
/// - Includes operation context
/// - Optionally includes additional details
/// - Never exposes sensitive information
/// 
/// Error Logging:
/// - LogError: Logs exceptions with full context
/// - Includes timestamp, context, exception type, message, and stack trace
/// - Currently uses console output (can be replaced with proper logging framework)
/// - Preserves full exception information for debugging
/// 
/// Response Creation:
/// - CreateErrorResponse: Creates standardized error response tuple
/// - CreateSuccessResponse: Creates standardized success response tuple
/// - Consistent format across all operations
/// - Optional error codes for categorization
/// 
/// Safe Execution:
/// - SafeExecute: Wraps synchronous operations with error handling
/// - SafeExecuteAsync: Wraps asynchronous operations with error handling
/// - Catches all exceptions
/// - Logs errors automatically
/// - Returns default value on error (prevents crashes)
/// 
/// Logging Implementation:
/// - Currently uses Console.Error.WriteLine
/// - In production, should use proper logging framework (Serilog, NLog, etc.)
/// - Logs include:
///   - Timestamp (UTC)
///   - Context (where error occurred)
///   - Additional information (if provided)
///   - Exception type and message
///   - Full stack trace
/// 
/// Example Usage:
/// ```csharp
/// // Format error message
/// var errorMsg = ErrorHandler.FormatErrorMessage("creating event", "Invalid date");
/// 
/// // Log error
/// try {
///     // operation
/// } catch (Exception ex) {
///     ErrorHandler.LogError(ex, "EventService.CreateEvent", "Failed to create event");
/// }
/// 
/// // Create error response
/// var (success, message) = ErrorHandler.CreateErrorResponse("Event not found", "EVENT_404");
/// 
/// // Safe execution
/// var result = ErrorHandler.SafeExecute(
///     () => riskyOperation(),
///     "Processing user request"
/// );
/// 
/// // Safe async execution
/// var result = await ErrorHandler.SafeExecuteAsync(
///     async () => await riskyOperationAsync(),
///     "Processing async operation"
/// );
/// ```
/// 
/// Best Practices:
/// - Always log errors with context
/// - Never expose sensitive information in error messages
/// - Use safe execution wrappers for risky operations
/// - Provide user-friendly error messages
/// - Include error codes for API responses
/// - Replace console logging with proper logging framework in production
/// </remarks>
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

