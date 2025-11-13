namespace CampusEvents.Models;

public enum CarType
{
    mini,
    car,
    van,
    limo,
    bus
}

public class Drivers
{
    public required int UserId { get; set; }
    public int EventId { get; set; }
    public required int Capacity { get; set; }
    public UserRole Role { get; set; }
    public CarType cartype { get; set; }
    // Navigation properties
    public Event Event { get; set; } = null!;
    public ICollection<User> users { get; set; } = new List<User>();
}