namespace CampusEvents.Models;

/// <summary>
/// Represents an organization in the Campus Events system.
/// Organizations are groups that can host events and have associated organizers.
/// </summary>
/// <remarks>
/// Organizations provide a way to group related events and organizers together.
/// They are optional - events and organizers can exist without an organization.
/// 
/// Key Features:
/// - Organizations can host multiple events
/// - Multiple organizers can belong to the same organization
/// - Organizations provide branding and grouping for events
/// 
/// Business Rules:
/// - Organizations are created by administrators
/// - Events can optionally be associated with an organization
/// - Organizers can optionally belong to an organization
/// - Organization association helps users discover related events
/// 
/// Relationships:
/// - One-to-Many: Organization → Events (organization hosts events)
/// - One-to-Many: Organization → Users (organizers belong to organization)
/// 
/// Use Cases:
/// - University departments (e.g., "Computer Science Department")
/// - Student clubs (e.g., "Tech Innovators Club")
/// - External organizations (e.g., "Industry Partners")
/// 
/// Example Usage:
/// ```csharp
/// var organization = new Organization
/// {
///     Name = "Tech Innovators Club",
///     Description = "Exploring cutting-edge technology and innovation",
///     CreatedAt = DateTime.UtcNow
/// };
/// 
/// // Link organizer to organization
/// organizer.OrganizationId = organization.Id;
/// 
/// // Link event to organization
/// event.OrganizationId = organization.Id;
/// ```
/// </remarks>
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
