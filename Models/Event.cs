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
    public int? CategoryId { get; set; }
    public bool IsApproved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? ImageUrl { get; set; }

    // Navigation properties
    public User Organizer { get; set; } = null!;
    public Organization? Organization { get; set; }
    public Category? CategoryEntity { get; set; }
    public EventAnalytics? Analytics { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<SavedEvent> SavedByUsers { get; set; } = new List<SavedEvent>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
