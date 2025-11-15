namespace CampusEvents.Services;

/// <summary>
/// Utility class for file operations and validation
/// Provides file handling helpers for uploads and file management
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Validates file size against maximum allowed size
    /// </summary>
    /// <param name="fileSize">File size in bytes</param>
    /// <param name="maxSize">Maximum allowed size in bytes</param>
    /// <returns>True if file size is valid</returns>
    public static bool IsValidFileSize(long fileSize, long maxSize)
    {
        return fileSize > 0 && fileSize <= maxSize;
    }

    /// <summary>
    /// Validates file extension against allowed extensions
    /// </summary>
    /// <param name="fileName">File name to validate</param>
    /// <param name="allowedExtensions">Array of allowed extensions (with or without dot)</param>
    /// <returns>True if file extension is allowed</returns>
    public static bool IsValidFileExtension(string? fileName, string[] allowedExtensions)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        
        return allowedExtensions.Any(ext =>
        {
            var normalizedExt = ext.StartsWith(".") ? ext : $".{ext}";
            return normalizedExt.Equals(extension, StringComparison.OrdinalIgnoreCase);
        });
    }

    /// <summary>
    /// Gets safe file name by removing invalid characters
    /// </summary>
    /// <param name="fileName">Original file name</param>
    /// <returns>Safe file name</returns>
    public static string GetSafeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "file";

        var invalidChars = System.IO.Path.GetInvalidFileNameChars();
        var safeName = new string(fileName
            .Where(c => !invalidChars.Contains(c))
            .ToArray());

        return safeName.Truncate(255, "");
    }

    /// <summary>
    /// Validates CSV file format
    /// </summary>
    /// <param name="filePath">Path to CSV file</param>
    /// <returns>True if file appears to be valid CSV</returns>
    public static bool IsValidCsvFile(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
            return false;

        try
        {
            var lines = System.IO.File.ReadLines(filePath).Take(5);
            return lines.Any(line => line.Contains(','));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets MIME type for a file based on extension
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <returns>MIME type string</returns>
    public static string GetMimeType(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "application/octet-stream";

        var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}

