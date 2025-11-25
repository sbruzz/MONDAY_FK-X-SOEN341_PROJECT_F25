namespace CampusEvents.Models;

/// <summary>
/// Represents a saved event in a user's personal calendar.
/// Acts as a many-to-many join table between Users and Events, allowing users
/// to save events to their personal calendar for later reference.
/// </summary>
/// <remarks>
/// This entity implements a many-to-many relationship between Users and Events:
/// - One user can save multiple events
/// - One event can be saved by multiple users
/// 
/// Key Features:
/// - Composite primary key (UserId, EventId) ensures uniqueness
/// - Tracks when the event was saved (SavedAt timestamp)
/// - Provides navigation properties for easy access to User and Event
/// 
/// Use Cases:
/// - Users can save interesting events to their personal calendar
/// - Users can view all their saved events in one place
/// - Organizers can see how many users are interested in their events
/// 
/// Database Configuration:
/// - Composite primary key configured in AppDbContext.OnModelCreating()
/// - Foreign keys to User and Event tables
/// - Indexed for efficient queries on UserId and EventId
/// 
/// Example Usage:
/// ```csharp
/// // Save an event for a user
/// var savedEvent = new SavedEvent
/// {
///     UserId = userId,
///     EventId = eventId,
///     SavedAt = DateTime.UtcNow
/// };
/// _context.SavedEvents.Add(savedEvent);
/// 
/// // Get all saved events for a user
/// var savedEvents = await _context.SavedEvents
///     .Include(se => se.Event)
///     .Where(se => se.UserId == userId)
///     .ToListAsync();
/// ```
/// </remarks>
public class SavedEvent
{
    /// <summary>
    /// Foreign key to User table - the user who saved this event
    /// Part of composite primary key
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Foreign key to Event table - the event that was saved
    /// Part of composite primary key
    /// </summary>
    public int EventId { get; set; }
    
    /// <summary>
    /// Timestamp when the event was saved to the user's calendar
    /// </summary>
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    
    /// <summary>
    /// User who saved this event
    /// </summary>
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Event that was saved
    /// </summary>
    public Event Event { get; set; } = null!;
}
