namespace CampusEvents.Models;

public enum RentalStatus
{
    Available,
    Rented,
    Disabled
}

public class Room
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Location { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public bool IsEnabled { get; set; } = true; // Admin can disable
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}

public class Rental
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int UserId { get; set; } // User who rented (student or organizer)
    public int? EventId { get; set; } // Optional: if rental is for an event
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public RentalStatus Status { get; set; } = RentalStatus.Rented;
    public string? Purpose { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Room Room { get; set; } = null!;
    public User User { get; set; } = null!;
    public Event? Event { get; set; }
}

