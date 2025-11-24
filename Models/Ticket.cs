namespace CampusEvents.Models;

/// <summary>
/// Represents a ticket for an event
/// Contains ticket information, QR code, and redemption status
/// </summary>
public class Ticket
{
    /// <summary>
    /// Unique identifier for the ticket
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to Event table - the event this ticket is for
    /// </summary>
    public int EventId { get; set; }
    
    /// <summary>
    /// Foreign key to User table - the user who claimed/purchased this ticket
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Unique code for the ticket (used for QR code generation)
    /// </summary>
    public required string UniqueCode { get; set; }
    
    /// <summary>
    /// Base64 encoded QR code image for event entry validation
    /// </summary>
    public string? QrCodeImage { get; set; }
    
    /// <summary>
    /// Timestamp when the ticket was claimed/purchased
    /// </summary>
    public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the ticket was redeemed at the event (null if not redeemed)
    /// </summary>
    public DateTime? RedeemedAt { get; set; }
    
    /// <summary>
    /// Whether the ticket has been redeemed/used at the event
    /// </summary>
    public bool IsRedeemed { get; set; } = false;

    // Navigation properties
    
    /// <summary>
    /// Event this ticket is for
    /// </summary>
    public Event Event { get; set; } = null!;
    
    /// <summary>
    /// User who owns this ticket
    /// </summary>
    public User User { get; set; } = null!;
}
