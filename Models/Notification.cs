namespace CampusEvents.Models;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? EventId { get; set; }
    public NotificationType Type { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Event? Event { get; set; }
}
