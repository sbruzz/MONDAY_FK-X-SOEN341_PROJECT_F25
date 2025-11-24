namespace CampusEvents.Models;

/// <summary>
/// Represents an organization in the Campus Events system
/// Organizations can be associated with events and organizers
/// </summary>
public class Organization
{
    /// <summary>
    /// Unique identifier for the organization
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the organization
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Detailed description of the organization
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Timestamp when the organization was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    
    /// <summary>
    /// Events associated with this organization
    /// </summary>
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
