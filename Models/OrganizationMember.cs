namespace CampusEvents.Models;

public class OrganizationMember
{
    public int OrganizationId { get; set; }
    public int UserId { get; set; }
    public OrganizationRole Role { get; set; } = OrganizationRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public User User { get; set; } = null!;
}
