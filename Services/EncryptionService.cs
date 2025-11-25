using System.Security.Cryptography;
using System.Text;

namespace CampusEvents.Services;

/// <summary>
/// Encryption service for sensitive data at rest.
/// Provides AES-256 encryption for driver license numbers, license plates, and other sensitive information.
/// </summary>
/// <remarks>
/// This service implements AES-256-CBC encryption with PKCS7 padding for encrypting
/// sensitive data before storing it in the database. This ensures that even if the
/// database is compromised, sensitive information remains protected.
/// 
/// Encryption Algorithm:
/// - Algorithm: AES (Advanced Encryption Standard)
/// - Key Size: 256 bits (32 bytes)
/// - Mode: CBC (Cipher Block Chaining)
/// - Padding: PKCS7
/// - IV (Initialization Vector): 128 bits (16 bytes), derived from key
/// 
/// Key Derivation:
/// - Encryption key is derived from configuration string using SHA256
/// - IV is derived from key string + "_iv" using SHA256, then truncated to 16 bytes
/// - This ensures deterministic IV generation while maintaining security
/// 
/// Data Protected:
/// - Driver license numbers (DriverLicenseNumber field)
/// - License plates (LicensePlate field)
/// - Any other sensitive data requiring encryption
/// 
/// Key Management:
/// - Encryption key loaded from configuration (appsettings.json or environment variables)
/// - Minimum key length: 32 characters (256 bits after SHA256 hashing)
/// - In production: Use environment variables or Azure Key Vault
/// - Never commit keys to version control
/// 
/// Security Considerations:
/// - Same plaintext always produces same ciphertext (deterministic encryption)
/// - IV is derived deterministically (not random per encryption)
/// - For production, consider using random IVs stored with ciphertext
/// - Key rotation requires re-encryption of all encrypted data
/// 
/// Performance:
/// - Registered as Singleton service (stateless, thread-safe)
/// - AES operations are fast and efficient
/// - No database access required
/// 
/// Error Handling:
/// - Decryption failures return empty string (data may be corrupted or wrong key)
/// - Encryption of null/empty strings returns as-is
/// - All exceptions are caught and handled gracefully
/// 
/// Example Usage:
/// ```csharp
/// // Encrypt sensitive data before storage
/// var encryptedLicense = _encryptionService.EncryptLicenseNumber("D12345678");
/// driver.DriverLicenseNumber = encryptedLicense;
/// 
/// // Decrypt when needed (admin view only)
/// var decryptedLicense = _encryptionService.DecryptLicenseNumber(
///     driver.DriverLicenseNumber
/// );
/// ```
/// 
/// Production Recommendations:
/// - Use Azure Key Vault or similar for key storage
/// - Implement key rotation strategy
/// - Consider using random IVs for better security
/// - Monitor encryption/decryption performance
/// - Audit access to decrypted data
/// </remarks>
public class EncryptionService
{
    /// <summary>
    /// AES encryption key (256 bits = 32 bytes).
    /// Derived from configuration string using SHA256.
    /// </summary>
    private readonly byte[] _key;
    
    /// <summary>
    /// Initialization Vector (IV) for AES-CBC mode (128 bits = 16 bytes).
    /// Derived from key string + "_iv" using SHA256, then truncated.
    /// </summary>
    private readonly byte[] _iv;

    /// <summary>
    /// Initializes a new instance of EncryptionService.
    /// </summary>
    /// <param name="configuration">Application configuration for accessing encryption key.
    /// Key is read from "Security:EncryptionKey" configuration path.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if encryption key is not configured or is too short (less than 32 characters).
    /// </exception>
    /// <remarks>
    /// Configuration Priority:
    /// 1. Environment variables (SECURITY__ENCRYPTIONKEY) - highest priority
    /// 2. appsettings.json (Security:EncryptionKey) - development
    /// 3. User Secrets - development
    /// 
    /// Key Generation:
    /// Generate a secure key using: openssl rand -base64 32
    /// This produces a 44-character Base64-encoded string (32 bytes = 256 bits)
    /// 
    /// Key Derivation:
    /// - Encryption key: SHA256 hash of key string (always 32 bytes)
    /// - IV: First 16 bytes of SHA256 hash of (key string + "_iv")
    /// 
    /// Security Requirements:
    /// - Key must be at least 32 characters (ensures 256-bit key after hashing)
    /// - Use cryptographically secure random key generation
    /// - Store keys securely (environment variables, key vault)
    /// - Never commit keys to version control
    /// - Use different keys for each environment (dev, staging, prod)
    /// </remarks>
    public EncryptionService(IConfiguration configuration)
    {
        // Get encryption key from config (should be in environment variables for production)
        var keyString = configuration["Security:EncryptionKey"]
            ?? throw new InvalidOperationException(
                "Encryption key not configured. Set Security:EncryptionKey in appsettings.json or " +
                "use environment variable SECURITY__ENCRYPTIONKEY. " +
                "Generate with: openssl rand -base64 32");

        // Validate key length
        // Minimum 32 characters ensures 256-bit key after SHA256 hashing
        if (keyString.Length < 32)
        {
            throw new InvalidOperationException(
                "Encryption key must be at least 32 characters. " +
                "Current length: " + keyString.Length);
        }

        // Derive encryption key and IV from the configured key string
        // Using SHA256 ensures consistent 32-byte key regardless of input length
        using var sha256 = SHA256.Create();
        
        // Encryption key: SHA256 hash of key string (always 32 bytes)
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
        
        // IV: First 16 bytes of SHA256 hash of (key string + "_iv")
        // This provides deterministic IV generation while maintaining security
        var ivHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString + "_iv"));
        _iv = ivHash.Take(16).ToArray(); // AES requires 16-byte IV
    }

    /// <summary>
    /// Encrypts a plaintext string
    /// </summary>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Decrypts an encrypted string
    /// </summary>
    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return encryptedText;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            // If decryption fails, return empty string (data might be corrupted or wrong key)
            return string.Empty;
        }
    }

    /// <summary>
    /// Encrypts a license plate for storage
    /// </summary>
    public string EncryptLicensePlate(string licensePlate)
    {
        return Encrypt(licensePlate?.ToUpperInvariant() ?? "");
    }

    /// <summary>
    /// Decrypts a license plate from storage
    /// </summary>
    public string DecryptLicensePlate(string encryptedPlate)
    {
        return Decrypt(encryptedPlate);
    }

    /// <summary>
    /// Encrypts a driver's license number for storage
    /// </summary>
    public string EncryptLicenseNumber(string licenseNumber)
    {
        return Encrypt(licenseNumber?.ToUpperInvariant() ?? "");
    }

    /// <summary>
    /// Decrypts a driver's license number from storage
    /// </summary>
    public string DecryptLicenseNumber(string encryptedNumber)
    {
        return Decrypt(encryptedNumber);
    }
}
