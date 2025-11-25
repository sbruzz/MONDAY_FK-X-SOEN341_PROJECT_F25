using System.Text.RegularExpressions;

namespace CampusEvents.Services;

/// <summary>
/// Validates Canadian driver's license numbers and license plates.
/// Supports all Canadian provinces and territories with province-specific format validation.
/// </summary>
/// <remarks>
/// This service provides validation for Canadian driver's licenses and license plates
/// across all 13 provinces and territories. Each province/territory has unique format
/// requirements that are enforced through regex pattern matching.
/// 
/// Supported Provinces/Territories:
/// - ON (Ontario)
/// - QC (Quebec)
/// - BC (British Columbia)
/// - AB (Alberta)
/// - SK (Saskatchewan)
/// - MB (Manitoba)
/// - NS (Nova Scotia)
/// - NB (New Brunswick)
/// - PE (Prince Edward Island)
/// - NL (Newfoundland and Labrador)
/// - YT (Yukon)
/// - NT (Northwest Territories)
/// - NU (Nunavut)
/// 
/// Validation Features:
/// - Format validation using regex patterns
/// - Province-specific format checking
/// - Case-insensitive matching where applicable
/// - Format examples for user guidance
/// - Detailed error messages with expected formats
/// 
/// Note: This validation checks format only, not authenticity or validity of the license.
/// For production use, consider integrating with official license verification services.
/// </remarks>
public class LicenseValidationService
{
    // Driver's License Patterns by Province/Territory
    private static readonly Dictionary<string, (Regex Pattern, string Format)> LicensePatterns = new()
    {
        // Ontario: 1 letter + 4 digits + 5 digits + 5 digits (LDDDD-DDDDD-DDDDD)
        ["ON"] = (new Regex(@"^[A-Z]\d{4}-?\d{5}-?\d{5}$", RegexOptions.IgnoreCase), "L1234-56789-01234"),

        // Quebec: 1 letter + 4 digits + 6 digits + 2 digits (LDDDD-DDDDDD-DD)
        ["QC"] = (new Regex(@"^[A-Z]\d{4}-?\d{6}-?\d{2}$", RegexOptions.IgnoreCase), "L1234-567890-12"),

        // British Columbia: 7 digits (1234567)
        ["BC"] = (new Regex(@"^\d{7}$"), "1234567"),

        // Alberta: 6-9 digits (123456 or 123456789)
        ["AB"] = (new Regex(@"^\d{6,9}$"), "123456 or 123456789"),

        // Saskatchewan: 8 digits (12345678)
        ["SK"] = (new Regex(@"^\d{8}$"), "12345678"),

        // Manitoba: 12 alphanumeric characters
        ["MB"] = (new Regex(@"^[A-Z0-9]{12}$", RegexOptions.IgnoreCase), "ABC123DEF456"),

        // Nova Scotia: 5 letters + 9 digits (LLLLL-DDDDDDDDD)
        ["NS"] = (new Regex(@"^[A-Z]{5}-?\d{9}$", RegexOptions.IgnoreCase), "ABCDE-123456789"),

        // New Brunswick: 7 digits (1234567)
        ["NB"] = (new Regex(@"^\d{7}$"), "1234567"),

        // Prince Edward Island: 6 digits or 1-6 digits (123456 or 1-123456)
        ["PE"] = (new Regex(@"^\d{1,6}$"), "123456"),

        // Newfoundland and Labrador: 1 letter + 9 digits (L123456789)
        ["NL"] = (new Regex(@"^[A-Z]\d{9}$", RegexOptions.IgnoreCase), "L123456789"),

        // Yukon: 6 digits (123456)
        ["YT"] = (new Regex(@"^\d{6}$"), "123456"),

        // Northwest Territories: 6 digits (123456)
        ["NT"] = (new Regex(@"^\d{6}$"), "123456"),

        // Nunavut: 6 digits (123456)
        ["NU"] = (new Regex(@"^\d{6}$"), "123456")
    };

    // License Plate Patterns by Province/Territory
    private static readonly Dictionary<string, (Regex Pattern, string Format)> PlatePatterns = new()
    {
        // Ontario: LLLL-123 or 1234-LLL or various other formats
        ["ON"] = (new Regex(@"^([A-Z]{4}-?\d{3}|\d{4}-?[A-Z]{3}|[A-Z]{2,4}\d{2,4})$", RegexOptions.IgnoreCase), "ABCD-123 or 1234-ABC"),

        // Quebec: L12-LLL or L12-L1L
        ["QC"] = (new Regex(@"^[A-Z]\d{2}-?[A-Z]{1,3}\d?$", RegexOptions.IgnoreCase), "A12-ABC or A12-B1C"),

        // British Columbia: LL1-11L or 111-LLL
        ["BC"] = (new Regex(@"^([A-Z]{2}\d-?\d{2}[A-Z]|\d{3}-?[A-Z]{3})$", RegexOptions.IgnoreCase), "AB1-23C or 123-ABC"),

        // Alberta: LLL-1234 or 111-LLL
        ["AB"] = (new Regex(@"^([A-Z]{3}-?\d{4}|\d{3}-?[A-Z]{3})$", RegexOptions.IgnoreCase), "ABC-1234 or 123-ABC"),

        // Saskatchewan: 123-LLL
        ["SK"] = (new Regex(@"^\d{3}-?[A-Z]{3}$", RegexOptions.IgnoreCase), "123-ABC"),

        // Manitoba: LLL-123
        ["MB"] = (new Regex(@"^[A-Z]{3}-?\d{3}$", RegexOptions.IgnoreCase), "ABC-123"),

        // Nova Scotia: LLL-111
        ["NS"] = (new Regex(@"^[A-Z]{3}-?\d{3}$", RegexOptions.IgnoreCase), "ABC-123"),

        // New Brunswick: LLL-111
        ["NB"] = (new Regex(@"^[A-Z]{3}-?\d{3}$", RegexOptions.IgnoreCase), "ABC-123"),

        // Prince Edward Island: LLL-111
        ["PE"] = (new Regex(@"^[A-Z]{3}-?\d{3}$", RegexOptions.IgnoreCase), "ABC-123"),

        // Newfoundland and Labrador: LLL-111
        ["NL"] = (new Regex(@"^[A-Z]{3}-?\d{3}$", RegexOptions.IgnoreCase), "ABC-123"),

        // Yukon: 12345 or 1-12345
        ["YT"] = (new Regex(@"^\d{1,5}$"), "12345"),

        // Northwest Territories: 12345
        ["NT"] = (new Regex(@"^\d{5}$"), "12345"),

        // Nunavut: 12345
        ["NU"] = (new Regex(@"^\d{5}$"), "12345")
    };

    public static readonly List<string> CanadianProvinces = new()
    {
        "ON", "QC", "BC", "AB", "SK", "MB", "NS", "NB", "PE", "NL", "YT", "NT", "NU"
    };

    /// <summary>
    /// Validates a Canadian driver's license number
    /// </summary>
    /// <param name="licenseNumber">The license number to validate</param>
    /// <param name="province">Two-letter province/territory code (e.g., "ON", "QC")</param>
    /// <returns>Validation result with success status and error message if invalid</returns>
    public ValidationResult ValidateDriverLicense(string? licenseNumber, string? province)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber))
        {
            return new ValidationResult(false, "Driver's license number is required.");
        }

        if (string.IsNullOrWhiteSpace(province))
        {
            return new ValidationResult(false, "Province/territory is required.");
        }

        var provinceCode = province.ToUpperInvariant();
        if (!LicensePatterns.ContainsKey(provinceCode))
        {
            return new ValidationResult(false, $"Invalid province/territory code: {province}. Must be one of: {string.Join(", ", CanadianProvinces)}");
        }

        var (pattern, format) = LicensePatterns[provinceCode];
        var cleanedLicense = licenseNumber.Trim();

        if (!pattern.IsMatch(cleanedLicense))
        {
            return new ValidationResult(false, $"Invalid driver's license format for {provinceCode}. Expected format: {format}");
        }

        return new ValidationResult(true, null);
    }

    /// <summary>
    /// Validates a Canadian license plate
    /// </summary>
    /// <param name="licensePlate">The license plate to validate</param>
    /// <param name="province">Two-letter province/territory code (e.g., "ON", "QC")</param>
    /// <returns>Validation result with success status and error message if invalid</returns>
    public ValidationResult ValidateLicensePlate(string? licensePlate, string? province)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return new ValidationResult(false, "License plate is required.");
        }

        if (string.IsNullOrWhiteSpace(province))
        {
            return new ValidationResult(false, "Province/territory is required.");
        }

        var provinceCode = province.ToUpperInvariant();
        if (!PlatePatterns.ContainsKey(provinceCode))
        {
            return new ValidationResult(false, $"Invalid province/territory code: {province}. Must be one of: {string.Join(", ", CanadianProvinces)}");
        }

        var (pattern, format) = PlatePatterns[provinceCode];
        var cleanedPlate = licensePlate.Trim().Replace(" ", "");

        if (!pattern.IsMatch(cleanedPlate))
        {
            return new ValidationResult(false, $"Invalid license plate format for {provinceCode}. Expected format: {format}");
        }

        return new ValidationResult(true, null);
    }

    /// <summary>
    /// Gets the expected format for a driver's license in a given province
    /// </summary>
    public string GetLicenseFormat(string province)
    {
        var provinceCode = province.ToUpperInvariant();
        return LicensePatterns.ContainsKey(provinceCode)
            ? LicensePatterns[provinceCode].Format
            : "Unknown";
    }

    /// <summary>
    /// Gets the expected format for a license plate in a given province
    /// </summary>
    public string GetPlateFormat(string province)
    {
        var provinceCode = province.ToUpperInvariant();
        return PlatePatterns.ContainsKey(provinceCode)
            ? PlatePatterns[provinceCode].Format
            : "Unknown";
    }
}

/// <summary>
/// Result of a validation operation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; }
    public string? ErrorMessage { get; }

    public ValidationResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }
}
