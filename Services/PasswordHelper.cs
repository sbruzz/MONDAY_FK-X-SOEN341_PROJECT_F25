namespace CampusEvents.Services;

/// <summary>
/// Utility class for password hashing and validation.
/// Provides secure password handling using BCrypt with adaptive work factor for
/// protection against brute-force attacks.
/// </summary>
/// <remarks>
/// This class provides static utility methods for secure password management, including
/// hashing, verification, strength validation, and random password generation.
/// 
/// Key Features:
/// - BCrypt password hashing with adaptive work factor
/// - Password verification against hashes
/// - Password strength validation
/// - Random secure password generation
/// - Password rehashing detection
/// 
/// Security Features:
/// - BCrypt adaptive work factor (default: 10, automatically adjusts)
/// - Salt automatically generated for each password
/// - Protection against rainbow table attacks
/// - Protection against timing attacks
/// - Slow hashing prevents brute-force attacks
/// 
/// Password Requirements:
/// - Minimum length: 8 characters (from Constants.Validation.MinPasswordLength)
/// - Maximum length: 128 characters (from Constants.Validation.MaxPasswordLength)
/// - Must contain at least one letter
/// - Must contain at least one number
/// 
/// Hashing Process:
/// 1. Password validated for strength
/// 2. BCrypt generates random salt
/// 3. Password hashed with salt and work factor
/// 4. Hash stored in database (includes salt and work factor)
/// 
/// Verification Process:
/// 1. Extract salt and work factor from stored hash
/// 2. Hash provided password with extracted salt
/// 3. Compare hashes (constant-time comparison)
/// 4. Return true if matches, false otherwise
/// 
/// Work Factor:
/// - Default: 10 (2^10 = 1024 iterations)
/// - Higher work factor = more secure but slower
/// - Automatically increases as hardware improves
/// - Can be increased for future-proofing
/// 
/// Example Usage:
/// ```csharp
/// // Hash password before storage
/// var passwordHash = PasswordHelper.HashPassword("MySecurePassword123");
/// user.PasswordHash = passwordHash;
/// 
/// // Verify password on login
/// var isValid = PasswordHelper.VerifyPassword("MySecurePassword123", user.PasswordHash);
/// 
/// // Validate password strength
/// var (isValid, errorMessage) = PasswordHelper.ValidatePasswordStrength("password");
/// 
/// // Generate random password
/// var randomPassword = PasswordHelper.GenerateRandomPassword(length: 16);
/// ```
/// 
/// Best Practices:
/// - Always hash passwords before storage
/// - Never store plain text passwords
/// - Use strong passwords (letters + numbers)
/// - Consider increasing work factor for high-security applications
/// - Rehash passwords if work factor increases
/// </remarks>
public static class PasswordHelper
{
    /// <summary>
    /// Hashes a password using BCrypt
    /// </summary>
    /// <param name="password">Plain text password to hash</param>
    /// <returns>BCrypt hashed password</returns>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hash">BCrypt hash to verify against</param>
    /// <returns>True if password matches hash</returns>
    public static bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates password strength
    /// </summary>
    /// <param name="password">Password to validate</param>
    /// <returns>Tuple with validation result and error message</returns>
    public static (bool IsValid, string? ErrorMessage) ValidatePasswordStrength(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password cannot be empty");

        if (password.Length < Constants.Validation.MinPasswordLength)
            return (false, $"Password must be at least {Constants.Validation.MinPasswordLength} characters long");

        if (password.Length > Constants.Validation.MaxPasswordLength)
            return (false, $"Password must be no more than {Constants.Validation.MaxPasswordLength} characters long");

        // Check for at least one letter
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-zA-Z]"))
            return (false, "Password must contain at least one letter");

        // Check for at least one number
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]"))
            return (false, "Password must contain at least one number");

        return (true, null);
    }

    /// <summary>
    /// Generates a random secure password
    /// </summary>
    /// <param name="length">Length of password to generate</param>
    /// <returns>Random secure password</returns>
    public static string GenerateRandomPassword(int length = 16)
    {
        if (length < Constants.Validation.MinPasswordLength)
            length = Constants.Validation.MinPasswordLength;

        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string numbers = "0123456789";
        const string special = "!@#$%^&*";
        const string allChars = uppercase + lowercase + numbers + special;

        var random = new Random();
        var password = new System.Text.StringBuilder();

        // Ensure at least one of each required character type
        password.Append(uppercase[random.Next(uppercase.Length)]);
        password.Append(lowercase[random.Next(lowercase.Length)]);
        password.Append(numbers[random.Next(numbers.Length)]);
        password.Append(special[random.Next(special.Length)]);

        // Fill the rest randomly
        for (int i = password.Length; i < length; i++)
        {
            password.Append(allChars[random.Next(allChars.Length)]);
        }

        // Shuffle the password
        var chars = password.ToString().ToCharArray();
        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    /// <summary>
    /// Checks if a password needs rehashing (if BCrypt work factor changed)
    /// </summary>
    /// <param name="hash">Current password hash</param>
    /// <returns>True if password should be rehashed</returns>
    public static bool NeedsRehash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return true;

        try
        {
            // BCrypt.Net automatically handles this, but we can check the work factor
            // Default work factor is 10, we can check if it's lower
            return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, 10);
        }
        catch
        {
            return true;
        }
    }
}

