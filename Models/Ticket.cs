namespace CampusEvents.Models;

public class Ticket
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public required string UniqueCode { get; set; }
    public string? QrCodeImage { get; set; } // Base64 encoded QR code image
    public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RedeemedAt { get; set; }
    public bool IsRedeemed { get; set; } = false;

    // Navigation properties
    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
}
