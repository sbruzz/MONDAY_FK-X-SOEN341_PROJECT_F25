using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CampusEvents.Services;

/// <summary>
/// Provides HMAC-based signing and verification for ticket QR codes.
/// Implements cryptographic security to prevent ticket forgery, guessing attacks, and replay attacks.
/// </summary>
/// <remarks>
/// This service is responsible for generating and validating secure tokens that are embedded
/// in QR codes for event tickets. The tokens use HMAC-SHA256 signing to ensure authenticity
/// and prevent tampering.
/// 
/// Security Features:
/// - HMAC-SHA256 signature prevents ticket forgery
/// - Expiry timestamps prevent replay attacks (tokens expire 24 hours after event)
/// - Constant-time comparison prevents timing attacks
/// - Version field allows for future token format changes
/// 
/// Token Structure:
/// The signed token is a JSON object containing:
/// - payload: Base64-encoded JSON with ticket information
/// - signature: Base64-encoded HMAC-SHA256 signature
/// 
/// Payload Contents:
/// - Version: Token version (currently 1)
/// - EventId: ID of the event
/// - TicketId: ID of the ticket
/// - UniqueCode: Unique ticket identifier
/// - Expiry: Expiry timestamp in UTC ticks
/// 
/// Key Management:
/// - Signing key loaded from configuration (appsettings.json or environment variables)
/// - Minimum key length: 32 characters (256 bits)
/// - In production: Use environment variables or Azure Key Vault
/// - Never commit keys to version control
/// 
/// Verification Process:
/// 1. Parse signed token JSON
/// 2. Decode payload and signature from Base64
/// 3. Compute HMAC-SHA256 signature of payload
/// 4. Compare computed signature with provided signature (constant-time)
/// 5. Parse payload JSON
/// 6. Verify token version
/// 7. Check expiry timestamp
/// 
/// Performance:
/// - Registered as Singleton service (stateless, thread-safe)
/// - HMAC operations are fast and efficient
/// - No database access required for signing/verification
/// 
/// Example Usage:
/// ```csharp
/// // Sign a ticket
/// var token = _ticketSigningService.SignTicket(
///     eventId: 1,
///     ticketId: 123,
///     uniqueCode: "TKT-2025-001",
///     eventDate: DateTime.UtcNow.AddDays(7)
/// );
/// 
/// // Verify a ticket
/// var result = _ticketSigningService.VerifyTicket(token);
/// if (result?.IsValid == true)
/// {
///     var ticketId = result.Payload.TicketId;
///     // Process valid ticket
/// }
/// ```
/// </remarks>
public class TicketSigningService
{
    /// <summary>
    /// Signing key for HMAC-SHA256 operations.
    /// Loaded from configuration at service initialization.
    /// Must be at least 32 characters (256 bits) for security.
    /// </summary>
    private readonly byte[] _signingKey;
    
    /// <summary>
    /// Current token version.
    /// Allows for future token format changes while maintaining backward compatibility.
    /// Increment this when changing token structure.
    /// </summary>
    private const int TOKEN_VERSION = 1;

    /// <summary>
    /// Initializes a new instance of TicketSigningService.
    /// </summary>
    /// <param name="configuration">Application configuration for accessing signing key.
    /// Key is read from "Security:TicketSigningKey" configuration path.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if signing key is not configured or is too short (less than 32 characters).
    /// </exception>
    /// <remarks>
    /// Configuration Priority:
    /// 1. Environment variables (SECURITY__TICKETSIGNINGKEY) - highest priority
    /// 2. appsettings.json (Security:TicketSigningKey) - development
    /// 3. User Secrets - development
    /// 
    /// Key Generation:
    /// Generate a secure key using: openssl rand -base64 48
    /// This produces a 64-character Base64-encoded string (48 bytes = 384 bits)
    /// 
    /// Security Requirements:
    /// - Key must be at least 32 characters (256 bits)
    /// - Use cryptographically secure random key generation
    /// - Store keys securely (environment variables, key vault)
    /// - Never commit keys to version control
    /// </remarks>
    public TicketSigningService(IConfiguration configuration)
    {
        // Environment variables override appsettings.json (use SECURITY__TICKETSIGNINGKEY)
        // For production: Use environment variables, User Secrets, or Azure Key Vault
        var keyString = configuration["Security:TicketSigningKey"]
            ?? throw new InvalidOperationException(
                "TicketSigningKey not configured. " +
                "Set Security:TicketSigningKey in appsettings.json (dev) or " +
                "SECURITY__TICKETSIGNINGKEY environment variable (prod). " +
                "Generate with: openssl rand -base64 48");

        // Convert key string to bytes for HMAC operations
        _signingKey = Encoding.UTF8.GetBytes(keyString);

        // Validate key length for security
        // Minimum 32 characters (256 bits) required for HMAC-SHA256
        if (_signingKey.Length < 32)
        {
            throw new InvalidOperationException(
                "TicketSigningKey must be at least 32 characters for security. " +
                "Current length: " + _signingKey.Length);
        }
    }

    /// <summary>
    /// Signs a ticket payload and returns a secure token for QR encoding.
    /// Payload includes: version, eventId, ticketId, uniqueCode, expiry timestamp.
    /// </summary>
    public string SignTicket(int eventId, int ticketId, string uniqueCode, DateTime eventDate)
    {
        // Token expires 24 hours after the event
        var expiry = eventDate.AddHours(24).ToUniversalTime();

        var payload = new TicketPayload
        {
            Version = TOKEN_VERSION,
            EventId = eventId,
            TicketId = ticketId,
            UniqueCode = uniqueCode,
            Expiry = expiry.Ticks
        };

        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);

        // Generate HMAC-SHA256 signature
        using var hmac = new HMACSHA256(_signingKey);
        var signature = hmac.ComputeHash(payloadBytes);

        // Combine: payload | signature
        var token = new SignedToken
        {
            Payload = Convert.ToBase64String(payloadBytes),
            Signature = Convert.ToBase64String(signature)
        };

        return JsonSerializer.Serialize(token);
    }

    /// <summary>
    /// Verifies a signed ticket token and returns the validated payload.
    /// Returns null if signature is invalid, expired, or tampered.
    /// </summary>
    public TicketValidationResult? VerifyTicket(string signedToken)
    {
        try
        {
            // Parse signed token
            var token = JsonSerializer.Deserialize<SignedToken>(signedToken);
            if (token == null || string.IsNullOrEmpty(token.Payload) || string.IsNullOrEmpty(token.Signature))
            {
                return TicketValidationResult.Invalid("Malformed token");
            }

            var payloadBytes = Convert.FromBase64String(token.Payload);
            var providedSignature = Convert.FromBase64String(token.Signature);

            // Verify HMAC signature
            using var hmac = new HMACSHA256(_signingKey);
            var computedSignature = hmac.ComputeHash(payloadBytes);

            if (!CryptographicOperations.FixedTimeEquals(computedSignature, providedSignature))
            {
                return TicketValidationResult.Invalid("Invalid signature - token may be forged or tampered");
            }

            // Parse payload
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            var payload = JsonSerializer.Deserialize<TicketPayload>(payloadJson);

            if (payload == null)
            {
                return TicketValidationResult.Invalid("Invalid payload");
            }

            // Check version
            if (payload.Version != TOKEN_VERSION)
            {
                return TicketValidationResult.Invalid($"Unsupported token version: {payload.Version}");
            }

            // Check expiry
            var expiryTime = new DateTime(payload.Expiry, DateTimeKind.Utc);
            if (DateTime.UtcNow > expiryTime)
            {
                return TicketValidationResult.Invalid($"Token expired on {expiryTime:yyyy-MM-dd HH:mm} UTC");
            }

            return TicketValidationResult.Valid(payload);
        }
        catch (Exception ex)
        {
            return TicketValidationResult.Invalid($"Token verification failed: {ex.Message}");
        }
    }

    private class SignedToken
    {
        public string Payload { get; set; } = "";
        public string Signature { get; set; } = "";
    }
}

/// <summary>
/// Ticket payload embedded in the signed token.
/// </summary>
public class TicketPayload
{
    public int Version { get; set; }
    public int EventId { get; set; }
    public int TicketId { get; set; }
    public string UniqueCode { get; set; } = "";
    public long Expiry { get; set; } // Ticks (UTC)
}

/// <summary>
/// Result of ticket verification.
/// </summary>
public class TicketValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public TicketPayload? Payload { get; set; }

    public static TicketValidationResult Valid(TicketPayload payload) => new()
    {
        IsValid = true,
        Payload = payload
    };

    public static TicketValidationResult Invalid(string error) => new()
    {
        IsValid = false,
        ErrorMessage = error
    };
}
