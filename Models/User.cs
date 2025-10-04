namespace CampusEvents.Models;

public enum UserRole
{
    Student,
    Organizer,
    Admin
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Name { get; set; }
    public UserRole Role { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<SavedEvent> SavedEvents { get; set; } = new List<SavedEvent>();
}
