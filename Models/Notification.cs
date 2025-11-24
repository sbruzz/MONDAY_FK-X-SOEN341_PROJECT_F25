using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusEvents.Models;

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    RoomDisabled,
    RentalApproved,
    RentalRejected,
    EventCancelled,
    EventUpdated
}

public class Notification
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required]
    public NotificationType Type { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? RelatedEntityId { get; set; }

    [StringLength(50)]
    public string? RelatedEntityType { get; set; }

    [StringLength(500)]
    public string? ActionUrl { get; set; }
}
