namespace CampusEvents.Models;

/// <summary>
/// Type of ticket for an event
/// </summary>
public enum TicketType
{
    /// <summary>
    /// Free ticket - no payment required
    /// </summary>
    Free,
    
    /// <summary>
    /// Paid ticket - requires payment
    /// </summary>
    Paid
}

/// <summary>
/// Represents a campus event in the system
/// Contains event details, capacity, pricing, and approval status
/// </summary>
public class Event
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Title/name of the event
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Detailed description of the event
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Date and time when the event occurs
    /// </summary>
    public DateTime EventDate { get; set; }
    
    /// <summary>
    /// Physical location or venue of the event
    /// </summary>
    public required string Location { get; set; }
    
    /// <summary>
    /// Maximum number of attendees allowed
    /// </summary>
    public int Capacity { get; set; }
    
    /// <summary>
    /// Number of tickets that have been issued
    /// </summary>
    public int TicketsIssued { get; set; } = 0;
    
    /// <summary>
    /// Type of ticket (Free or Paid)
    /// </summary>
    public TicketType TicketType { get; set; }
    
    /// <summary>
    /// Price per ticket (only used if TicketType is Paid)
    /// </summary>
    public decimal Price { get; set; } = 0;
    
    /// <summary>
    /// Category/type of event (e.g., "Academic", "Social", "Sports")
    /// </summary>
    public required string Category { get; set; }
    
    /// <summary>
    /// Foreign key to User table - the organizer who created this event
    /// </summary>
    public int OrganizerId { get; set; }
    
    /// <summary>
    /// Foreign key to Organization table (optional)
    /// </summary>
    public int? OrganizationId { get; set; }
    
    /// <summary>
    /// Approval status - events require admin approval before being visible
    /// </summary>
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    
    /// <summary>
    /// Timestamp when the event was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    
    /// <summary>
    /// User who organized this event
    /// </summary>
    public User Organizer { get; set; } = null!;
    
    /// <summary>
    /// Organization associated with this event (if any)
    /// </summary>
    public Organization? Organization { get; set; }
    
    /// <summary>
    /// Tickets issued for this event
    /// </summary>
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    
    /// <summary>
    /// Users who have saved this event to their calendar
    /// </summary>
    public ICollection<SavedEvent> SavedByUsers { get; set; } = new List<SavedEvent>();
    
    /// <summary>
    /// Carpool offers available for this event
    /// </summary>
    public ICollection<CarpoolOffer> CarpoolOffers { get; set; } = new List<CarpoolOffer>();
}
