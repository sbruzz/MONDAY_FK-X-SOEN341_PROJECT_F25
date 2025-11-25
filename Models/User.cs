using CampusEvents.Pages;

namespace CampusEvents.Models;

/// <summary>
/// Represents the role of a user in the system.
/// Determines what actions a user can perform and what features they can access.
/// </summary>
/// <remarks>
/// User roles implement role-based access control (RBAC) in the system.
/// Each role has specific permissions and capabilities:
/// 
/// Role Permissions:
/// - Student: Browse events, claim tickets, join carpools, rent rooms, save events
/// - Organizer: All student permissions + create events, manage rooms, validate tickets, offer carpools
/// - Admin: All permissions + approve users/events, manage drivers, system administration
/// 
/// Role Assignment:
/// - Role is set during account creation
/// - Cannot be changed by users (admin-only operation)
/// - Organizer accounts require admin approval before activation
/// </remarks>
public enum UserRole
{
    /// <summary>
    /// Student user role - can browse events, claim tickets, join carpools, rent rooms.
    /// This is the default role for most users in the system.
    /// </summary>
    Student,
    
    /// <summary>
    /// Organizer user role - can create events, manage rooms, offer carpools, validate tickets.
    /// Organizers have all student permissions plus event management capabilities.
    /// Requires admin approval before account activation.
    /// </summary>
    Organizer,
    
    /// <summary>
    /// Administrator user role - can approve users/events, manage drivers, system administration.
    /// Administrators have full system access and can perform all operations.
    /// </summary>
    Admin
}

/// <summary>
/// Represents the approval status of a user or event in the system.
/// Used to implement approval workflows for organizers and events.
/// </summary>
/// <remarks>
/// Approval Status Workflow:
/// - Pending: Initial state, awaiting admin review
/// - Approved: Approved by admin, now active in the system
/// - Rejected: Rejected by admin, not active
/// 
/// Usage:
/// - User.ApprovalStatus: For organizer accounts (students are auto-approved)
/// - Event.ApprovalStatus: For all events created by organizers
/// 
/// Business Rules:
/// - Students are automatically approved (ApprovalStatus.Approved)
/// - Organizers require admin approval (start as ApprovalStatus.Pending)
/// - Events require admin approval (start as ApprovalStatus.Pending)
/// - Only approved users/events are active in the system
/// </remarks>
public enum ApprovalStatus
{
    /// <summary>
    /// Pending approval - awaiting admin review.
    /// User/event is created but not yet active in the system.
    /// </summary>
    Pending,
    
    /// <summary>
    /// Approved - user/event is active and fully functional.
    /// Users can log in and use the system, events are visible to students.
    /// </summary>
    Approved,
    
    /// <summary>
    /// Rejected - user/event was denied by administrator.
    /// Users cannot log in, events are not visible to students.
    /// </summary>
    Rejected
}

/// <summary>
/// Represents a user account in the Campus Events system.
/// Contains user authentication credentials, profile information, role-specific data,
/// and navigation properties to related entities.
/// </summary>
/// <remarks>
/// The User entity is the central entity for user management in the system.
/// It supports three distinct user roles (Student, Organizer, Admin) with
/// role-specific properties and capabilities.
/// 
/// Key Features:
/// - Authentication: Email and BCrypt-hashed password
/// - Role-based access control (Student, Organizer, Admin)
/// - Approval workflow for organizers
/// - Role-specific profile information
/// - Comprehensive navigation properties
/// 
/// Student-Specific Properties:
/// - StudentId: 9-digit Concordia student ID
/// - Program: Academic program of study
/// - YearOfStudy: Year of study (e.g., "3rd Year")
/// - PhoneNumber: Contact phone number
/// 
/// Organizer-Specific Properties:
/// - OrganizationId: Associated organization
/// - Position: Position within organization
/// - Department: Department within organization
/// 
/// Business Rules:
/// - Email must be unique across all users (enforced by unique index)
/// - Password must be hashed using BCrypt (never stored in plain text)
/// - Organizer accounts require admin approval before activation
/// - Students are automatically approved
/// - Students can have at most one driver profile
/// - Organizers can have multiple driver profiles
/// 
/// Security:
/// - Passwords hashed with BCrypt (adaptive work factor)
/// - Email used as unique identifier for login
/// - Session-based authentication (user ID stored in session)
/// - Role stored in session for authorization checks
/// 
/// Relationships:
/// - One-to-Many: User → Events (as organizer)
/// - One-to-Many: User → Tickets (as owner)
/// - One-to-Many: User → Drivers (can have multiple for organizers)
/// - One-to-Many: User → RoomRentals (as renter)
/// - One-to-Many: User → Notifications (as recipient)
/// - Many-to-Many: User ↔ Event (via SavedEvent)
/// - Many-to-Many: User ↔ CarpoolOffer (via CarpoolPassenger)
/// - Many-to-One: User → Organization (for organizers)
/// 
/// Validation:
/// - Email: Required, unique, valid email format
/// - Password: Required, hashed with BCrypt
/// - Name: Required
/// - StudentId: 9 digits (if provided, for students)
/// 
/// Example Usage:
/// ```csharp
/// // Create student user
/// var student = new User
/// {
///     Email = "student@concordia.ca",
///     PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
///     Name = "John Doe",
///     Role = UserRole.Student,
///     ApprovalStatus = ApprovalStatus.Approved, // Auto-approved
///     StudentId = "40294756",
///     Program = "Computer Science",
///     YearOfStudy = "3rd Year"
/// };
/// 
/// // Create organizer user
/// var organizer = new User
/// {
///     Email = "organizer@concordia.ca",
///     PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
///     Name = "Jane Smith",
///     Role = UserRole.Organizer,
///     ApprovalStatus = ApprovalStatus.Pending, // Requires approval
///     OrganizationId = 1,
///     Position = "Event Coordinator"
/// };
/// ```
/// </remarks>
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
