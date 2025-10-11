using Microsoft.AspNetCore.Identity;

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

//inherit identity user browser knows id
public class User:IdentityUser
{
    //IdentityUser already has a native variables for these overriding caused issues
    //public string Id { get; set; }
    //public required string Email { get; set; }
    //public required string PasswordHash { get; set; }
    //public required string Name { get; set; }
    public UserRole Role { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
   
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<SavedEvent> SavedEvents { get; set; } = new List<SavedEvent>();
}
