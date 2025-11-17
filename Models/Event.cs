namespace CampusEvents.Models;

public enum TicketType
{
    Free,
    Paid
}

public class Event
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime EventDate { get; set; }
    public required string Location { get; set; }
    public int Capacity { get; set; }
    public int TicketsIssued { get; set; } = 0;
    public TicketType TicketType { get; set; }
    public decimal Price { get; set; } = 0;
    public required string Category { get; set; }
    public int OrganizerId { get; set; }
    public int? OrganizationId { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Organizer { get; set; } = null!;
    public Organization? Organization { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<SavedEvent> SavedByUsers { get; set; } = new List<SavedEvent>();
    public ICollection<CarpoolOffer> CarpoolOffers { get; set; } = new List<CarpoolOffer>();
}
