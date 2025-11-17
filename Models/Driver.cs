namespace CampusEvents.Models;

public enum DriverType
{
    Student,
    Organizer
}

public enum VehicleType
{
    Car,
    Van,
    SUV,
    Truck,
    Other
}

public class Driver
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? EventId { get; set; } // Optional: for event-specific drivers
    public int? OrganizerId { get; set; } // If created by organizer
    
    public DriverType Type { get; set; }
    public VehicleType VehicleType { get; set; }
    public int Capacity { get; set; } // Number of passengers
    public bool HasAccessibility { get; set; } // Wheelchair accessible, etc.
    public string? VehicleDescription { get; set; }
    public string? LicensePlate { get; set; }
    public string? ContactPhone { get; set; }
    
    // Status and history
    public bool IsActive { get; set; } = true;
    public bool IsMarkedByAdmin { get; set; } = false; // For security reasons
    public string? AdminNotes { get; set; }
    public int TotalRides { get; set; } = 0; // History tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Event? Event { get; set; }
    public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
}

public class Passenger
{
    public int Id { get; set; }
    public int DriverId { get; set; }
    public int UserId { get; set; } // Student who is a passenger
    public int EventId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Driver Driver { get; set; } = null!;
    public User User { get; set; } = null!;
    public Event Event { get; set; } = null!;
}

