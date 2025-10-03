namespace CampusEvents.Models;

public class SavedEvent
{
    public int UserId { get; set; }
    public int EventId { get; set; }
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Event Event { get; set; } = null!;
}
