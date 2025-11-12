namespace CampusEvents.Services;

/// <summary>
/// Utility class for creating standardized API responses
/// Provides consistent response formatting across the application
/// </summary>
public static class ResponseHelper
{
    /// <summary>
    /// Creates a successful response
    /// </summary>
    /// <param name="message">Success message</param>
    /// <param name="data">Optional data to include</param>
    /// <returns>Success response tuple</returns>
    public static (bool Success, string Message, T? Data) CreateSuccess<T>(string message, T? data = default)
    {
        return (true, message, data);
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errorCode">Optional error code</param>
    /// <returns>Error response tuple</returns>
    public static (bool Success, string Message) CreateError(string message, string? errorCode = null)
    {
        var fullMessage = message;
        if (!string.IsNullOrWhiteSpace(errorCode))
        {
            fullMessage = $"[{errorCode}] {message}";
        }
        return (false, fullMessage);
    }

    /// <summary>
    /// Creates a validation error response
    /// </summary>
    /// <param name="field">Field that failed validation</param>
    /// <param name="message">Validation error message</param>
    /// <returns>Error response tuple</returns>
    public static (bool Success, string Message) CreateValidationError(string field, string message)
    {
        return CreateError($"Validation failed for {field}: {message}", "VALIDATION_ERROR");
    }

    /// <summary>
    /// Creates a not found error response
    /// </summary>
    /// <param name="resourceType">Type of resource not found</param>
    /// <param name="identifier">Identifier of the resource</param>
    /// <returns>Error response tuple</returns>
    public static (bool Success, string Message) CreateNotFoundError(string resourceType, string identifier)
    {
        return CreateError($"{resourceType} with identifier '{identifier}' was not found", "NOT_FOUND");
    }

    /// <summary>
    /// Creates an unauthorized error response
    /// </summary>
    /// <param name="message">Optional custom message</param>
    /// <returns>Error response tuple</returns>
    public static (bool Success, string Message) CreateUnauthorizedError(string? message = null)
    {
        return CreateError(message ?? Constants.ErrorMessages.Unauthorized, "UNAUTHORIZED");
    }

    /// <summary>
    /// Creates a conflict error response (e.g., duplicate entry)
    /// </summary>
    /// <param name="message">Conflict description</param>
    /// <returns>Error response tuple</returns>
    public static (bool Success, string Message) CreateConflictError(string message)
    {
        return CreateError(message, "CONFLICT");
    }

    /// <summary>
    /// Wraps a result in a response
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    /// <param name="result">Result to wrap</param>
    /// <param name="successMessage">Success message</param>
    /// <param name="notFoundMessage">Message if result is null</param>
    /// <returns>Response tuple</returns>
    public static (bool Success, string Message, T? Data) WrapResult<T>(
        T? result,
        string successMessage,
        string notFoundMessage = "Resource not found")
    {
        if (result == null)
        {
            return (false, notFoundMessage, default(T));
        }

        return (true, successMessage, result);
    }
}

