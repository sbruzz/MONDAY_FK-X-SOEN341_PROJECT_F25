namespace CampusEvents.Models;

/// <summary>
/// Represents a saved event in a user's personal calendar
/// Acts as a many-to-many join table between Users and Events
/// </summary>
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
