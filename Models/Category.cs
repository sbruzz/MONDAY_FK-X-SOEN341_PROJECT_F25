namespace CampusEvents.Models;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
