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

    // Extended Student Information
    public string? StudentId { get; set; }
    public string? Program { get; set; }
    public string? YearOfStudy { get; set; }
    public string? PhoneNumber { get; set; }

    // Extended Organizer Information
    public int? OrganizationId { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }

    // Navigation properties
    public Organization? Organization { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<SavedEvent> SavedEvents { get; set; } = new List<SavedEvent>();
}
