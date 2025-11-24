namespace CampusEvents.Services;

/// <summary>
/// Utility class for password hashing and validation
/// Provides secure password handling using BCrypt
/// </summary>
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

