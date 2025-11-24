using CampusEvents.Pages;

namespace CampusEvents.Models;

/// <summary>
/// Represents the role of a user in the system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Student user role - can browse events, claim tickets, join carpools
    /// </summary>
    Student,
    
    /// <summary>
    /// Organizer user role - can create events, manage rooms, offer carpools
    /// </summary>
    Organizer,
    
    /// <summary>
    /// Administrator user role - can approve users/events, manage system
    /// </summary>
    Admin
}

/// <summary>
/// Represents the approval status of a user or event
/// </summary>
public enum ApprovalStatus
{
    /// <summary>
    /// Pending approval - awaiting admin review
    /// </summary>
    Pending,
    
    /// <summary>
    /// Approved - user/event is active
    /// </summary>
    Approved,
    
    /// <summary>
    /// Rejected - user/event was denied
    /// </summary>
    Rejected
}

/// <summary>
/// Represents a user in the Campus Events system
/// Contains user authentication, profile information, and role-specific data
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// User's email address (used for login)
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// BCrypt hashed password for authentication
    /// </summary>
    public required string PasswordHash { get; set; }
    
    /// <summary>
    /// User's full name
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// User's role in the system (Student, Organizer, or Admin)
    /// </summary>
    public UserRole Role { get; set; }
    
    /// <summary>
    /// Approval status for organizer accounts (requires admin approval)
    /// </summary>
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    
    /// <summary>
    /// Timestamp when the user account was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User activity history (stored as JSON string)
    /// </summary>
    public string history { get; set; } = "No History yet";

    // Extended Student Information
    
    /// <summary>
    /// Student ID (9-digit Concordia student ID)
    /// </summary>
    public string? StudentId { get; set; }
    
    /// <summary>
    /// Academic program of study
    /// </summary>
    public string? Program { get; set; }
    
    /// <summary>
    /// Year of study (e.g., "1st Year", "2nd Year")
    /// </summary>
    public string? YearOfStudy { get; set; }
    
    /// <summary>
    /// Contact phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    // Extended Organizer Information
    
    /// <summary>
    /// Foreign key to Organization table (for organizers)
    /// </summary>
    public int? OrganizationId { get; set; }
    
    /// <summary>
    /// Position/title within the organization
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Department within the organization
    /// </summary>
    public string? Department { get; set; }

    // Navigation properties
    
    /// <summary>
    /// Organization the user belongs to (for organizers)
    /// </summary>
    public Organization? Organization { get; set; }
    
    /// <summary>
    /// Driver profiles associated with this user (organizers can have multiple, students only one)
    /// </summary>
    public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
    
    /// <summary>
    /// Tickets claimed/purchased by this user
    /// </summary>
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    
    /// <summary>
    /// Events organized by this user (for organizers)
    /// </summary>
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    
    /// <summary>
    /// Events saved to user's personal calendar
    /// </summary>
    public ICollection<SavedEvent> SavedEvents { get; set; } = new List<SavedEvent>();
    
    /// <summary>
    /// Room rentals requested by this user
    /// </summary>
    public ICollection<RoomRental> RoomRentals { get; set; } = new List<RoomRental>();
    
    /// <summary>
    /// Carpool rides joined as a passenger
    /// </summary>
    public ICollection<CarpoolPassenger> CarpoolPassengers { get; set; } = new List<CarpoolPassenger>();
    
    /// <summary>
    /// Rooms managed by this user (for organizers)
    /// </summary>
    public ICollection<Room> ManagedRooms { get; set; } = new List<Room>();

    /// <summary>
    /// Notifications received by this user
    /// </summary>
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
