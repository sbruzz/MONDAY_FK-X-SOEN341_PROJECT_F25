namespace CampusEvents.Models;

public class QrScanLog
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int ScannedBy { get; set; } // UserId who performed the scan
    public ScanMethod ScanMethod { get; set; }
    public string? ScanLocation { get; set; }
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
    public bool IsValid { get; set; } = true;

    // Navigation properties
    public Ticket Ticket { get; set; } = null!;
    public User Scanner { get; set; } = null!;
}
