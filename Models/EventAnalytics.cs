namespace CampusEvents.Models;

public class EventAnalytics
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int ViewCount { get; set; } = 0;
    public int SaveCount { get; set; } = 0;
    public int TicketsSold { get; set; } = 0;
    public decimal Revenue { get; set; } = 0;
    public decimal AttendanceRate { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Event Event { get; set; } = null!;
}
