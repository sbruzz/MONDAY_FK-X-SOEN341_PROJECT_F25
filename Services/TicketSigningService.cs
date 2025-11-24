using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CampusEvents.Services;

/// <summary>
/// Provides HMAC-based signing and verification for ticket QR codes.
/// Prevents ticket forgery, guessing attacks, and replay attacks.
/// </summary>
public class TicketSigningService
{
    private readonly byte[] _signingKey;
    private const int TOKEN_VERSION = 1;

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

        _signingKey = Encoding.UTF8.GetBytes(keyString);

        if (_signingKey.Length < 32)
        {
            throw new InvalidOperationException("TicketSigningKey must be at least 32 characters for security");
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
