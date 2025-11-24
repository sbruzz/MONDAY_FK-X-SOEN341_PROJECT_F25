using System.Security.Cryptography;
using System.Text;

namespace CampusEvents.Services;

/// <summary>
/// Encryption service for sensitive data (license numbers, plates, etc.)
/// Uses AES-256 encryption with a key from configuration
/// </summary>
public class EncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IConfiguration configuration)
    {
        // Get encryption key from config (should be in environment variables for production)
        var keyString = configuration["Security:EncryptionKey"]
            ?? throw new InvalidOperationException(
                "Encryption key not configured. Set Security:EncryptionKey in appsettings.json or " +
                "use environment variable SECURITY__ENCRYPTIONKEY. " +
                "Generate with: openssl rand -base64 32");

        if (keyString.Length < 32)
        {
            throw new InvalidOperationException("Encryption key must be at least 32 characters");
        }

        // Derive key and IV from the configured key string
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
        _iv = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString + "_iv")).Take(16).ToArray();
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
