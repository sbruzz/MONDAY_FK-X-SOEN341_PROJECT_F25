namespace CampusEvents.Models;


public enum Room
{
    FR101,
    FR102,
    FR103,
    FR201,
    FR202,
    FR203
}



public class Rental
{
    public required int UserId { get; set; }
    public int EventId { get; set; }
    public required int Capacity { get; set; }
    public UserRole Role { get; set; }
    public required Room room { get; set; }
    public DateTime RentStart { get; set; }
    public DateTime Rentend { get; set; }

    // Navigation properties
    public Event Event { get; set; } = null!;
    public ICollection<User> users { get; set; } = new List<User>();
}