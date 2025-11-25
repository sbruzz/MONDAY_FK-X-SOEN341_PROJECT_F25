namespace CampusEvents.Models;

/// <summary>
/// Category/type of event for classification and filtering purposes.
/// Used to organize events by their nature and purpose, making it easier
/// for users to discover events of interest.
/// </summary>
/// <remarks>
/// Event categories help users filter and search for events based on their interests.
/// Organizers select the most appropriate category when creating an event.
/// 
/// Categories:
/// - Academic: Lectures, seminars, academic conferences
/// - Career: Career fairs, networking events, job workshops
/// - Competition: Hackathons, coding competitions, contests
/// - Concert: Musical performances, concerts, live music
/// - Cultural: Cultural celebrations, heritage events, diversity events
/// - Social: Social gatherings, parties, meetups
/// - Sports: Sporting events, tournaments, athletic competitions
/// - Workshop: Educational workshops, hands-on learning sessions
/// - Other: Events that don't fit into other categories
/// </remarks>
public enum EventCategory
{
    /// <summary>
    /// Academic events: lectures, seminars, academic conferences
    /// </summary>
    Academic,
    
    /// <summary>
    /// Career-related events: career fairs, networking, job workshops
    /// </summary>
    Career,
    
    /// <summary>
    /// Competitive events: hackathons, coding competitions, contests
    /// </summary>
    Competition,
    
    /// <summary>
    /// Musical events: concerts, performances, live music
    /// </summary>
    Concert,
    
    /// <summary>
    /// Cultural events: celebrations, heritage events, diversity events
    /// </summary>
    Cultural,
    
    /// <summary>
    /// Social events: gatherings, parties, meetups
    /// </summary>
    Social,
    
    /// <summary>
    /// Sports events: tournaments, athletic competitions, games
    /// </summary>
    Sports,
    
    /// <summary>
    /// Educational workshops: hands-on learning, skill-building sessions
    /// </summary>
    Workshop,
    
    /// <summary>
    /// Other events that don't fit into specific categories
    /// </summary>
    Other
}

/// <summary>
/// Type of ticket for an event, determining whether payment is required.
/// Used to differentiate between free events and paid events in the system.
/// </summary>
/// <remarks>
/// Ticket types affect:
/// - Ticket claiming process (free vs purchase flow)
/// - Payment processing (if applicable)
/// - Event pricing display
/// - Revenue tracking (for paid events)
/// </remarks>
public enum TicketType
{
    /// <summary>
    /// Free ticket - no payment required.
    /// Users can claim these tickets immediately without any payment process.
    /// </summary>
    Free,

    /// <summary>
    /// Paid ticket - requires payment.
    /// Users must complete a payment process (currently mock payment) to obtain these tickets.
    /// Price must be set when TicketType is Paid.
    /// </summary>
    Paid
}

/// <summary>
/// Represents a campus event in the system.
/// Contains comprehensive event details, capacity management, pricing information,
/// and approval workflow status.
/// </summary>
/// <remarks>
/// The Event entity is the central entity in the Campus Events system, representing
/// all events that can be attended by students. Events are created by organizers
/// and must be approved by administrators before being visible to students.
/// 
/// Key Features:
/// - Comprehensive event information (title, description, date, location)
/// - Capacity management (maximum attendees, tickets issued tracking)
/// - Pricing support (free or paid tickets)
/// - Category classification for filtering
/// - Approval workflow (Pending → Approved/Rejected)
/// - Organization association (optional)
/// 
/// Business Rules:
/// - Events require admin approval before being visible to students
/// - Event date must be in the future when created
/// - Capacity must be positive
/// - TicketsIssued cannot exceed Capacity
/// - Price must be set if TicketType is Paid
/// - Price must be non-negative
/// - Only approved events appear in student event listings
/// 
/// Relationships:
/// - One-to-Many with Tickets (one event has many tickets)
/// - Many-to-One with User (organizer who created the event)
/// - Many-to-One with Organization (optional organization association)
/// - Many-to-Many with User (via SavedEvent - users can save events)
/// - One-to-Many with CarpoolOffer (carpool rides for this event)
/// 
/// Validation:
/// - Title: Required, max 200 characters
/// - Description: Required, max 5000 characters
/// - EventDate: Must be in the future
/// - Location: Required
/// - Capacity: Must be positive integer
/// - Price: Must be non-negative, required if TicketType is Paid
/// 
/// Lifecycle:
/// 1. Created by organizer with ApprovalStatus.Pending
/// 2. Reviewed by administrator
/// 3. Approved or rejected
/// 4. If approved, visible to students
/// 5. Tickets can be claimed up to capacity
/// 6. Event occurs on EventDate
/// 7. Tickets can be validated via QR codes
/// 
/// Example Usage:
/// ```csharp
/// var event = new Event
/// {
///     Title = "Tech Conference 2025",
///     Description = "Annual technology conference",
///     EventDate = DateTime.UtcNow.AddDays(30),
///     Location = "Hall Building, Room H-110",
///     Capacity = 200,
///     TicketType = TicketType.Free,
///     Price = 0,
///     Category = EventCategory.Academic,
///     OrganizerId = organizerId,
///     ApprovalStatus = ApprovalStatus.Pending
/// };
/// ```
/// </remarks>
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
    /// Category/type of event
    /// </summary>
    public EventCategory Category { get; set; }
    
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
