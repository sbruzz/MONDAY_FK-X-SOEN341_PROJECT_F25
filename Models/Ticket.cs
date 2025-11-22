namespace CampusEvents.Models;

public class Ticket
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    /// <summary>
    /// Canonical source of truth for ticket verification.
    /// QR codes are generated on-demand from this value.
    /// </summary>
    public required string UniqueCode { get; set; }
    public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RedeemedAt { get; set; }
    public bool IsRedeemed { get; set; } = false;

    // Navigation properties
    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
}
