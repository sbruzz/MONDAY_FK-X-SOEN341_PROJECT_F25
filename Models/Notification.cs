using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusEvents.Models;

/// <summary>
/// Type of notification for categorization and display purposes.
/// Determines notification appearance and priority in the user interface.
/// </summary>
/// <remarks>
/// Notification types help users quickly identify the nature and importance of notifications.
/// Different types may have different display styles, icons, or priority levels.
/// 
/// Notification Categories:
/// - Info: General information notifications
/// - Success: Success/confirmation notifications
/// - Warning: Warning messages
/// - Error: Error messages
/// - System: System-generated notifications (RoomDisabled, RentalApproved, etc.)
/// </remarks>
public enum NotificationType
{
    /// <summary>
    /// General information notification
    /// </summary>
    Info,
    
    /// <summary>
    /// Success/confirmation notification
    /// </summary>
    Success,
    
    /// <summary>
    /// Warning notification
    /// </summary>
    Warning,
    
    /// <summary>
    /// Error notification
    /// </summary>
    Error,
    
    /// <summary>
    /// Room has been disabled by administrator
    /// </summary>
    RoomDisabled,
    
    /// <summary>
    /// Room rental request has been approved
    /// </summary>
    RentalApproved,
    
    /// <summary>
    /// Room rental request has been rejected
    /// </summary>
    RentalRejected,
    
    /// <summary>
    /// Event has been cancelled
    /// </summary>
    EventCancelled,
    
    /// <summary>
    /// Event details have been updated
    /// </summary>
    EventUpdated
}

/// <summary>
/// Notification entity for user notifications.
/// Represents a notification sent to a user about various system events and state changes.
/// </summary>
/// <remarks>
/// The Notification entity provides a comprehensive notification system for informing users
/// about important events, state changes, and system updates.
/// 
/// Key Features:
/// - User-specific notifications
/// - Multiple notification types (Info, Success, Warning, Error, System)
/// - Read/unread status tracking
/// - Related entity linking (for navigation)
/// - Action URLs (for direct navigation to relevant pages)
/// - Timestamp tracking
/// 
/// Business Rules:
/// - Notifications are created by services (NotificationService, CarpoolService, etc.)
/// - Users can only view their own notifications
/// - Notifications can be marked as read
/// - Old read notifications can be deleted
/// - Indexed for efficient queries (UserId, IsRead, CreatedAt)
/// 
/// Notification Types:
/// - Info: General information
/// - Success: Success messages
/// - Warning: Warning messages
/// - Error: Error messages
/// - System: System-generated (RoomDisabled, RentalApproved, etc.)
/// 
/// Related Entity Linking:
/// - RelatedEntityId: ID of related entity (e.g., rental ID, event ID)
/// - RelatedEntityType: Type of related entity (e.g., "RoomRental", "Event")
/// - Used for navigation and context in the UI
/// 
/// Action URLs:
/// - Optional URL for direct navigation to relevant page
/// - Examples: "/Student/Rentals", "/Student/Tickets"
/// - Helps users quickly access related content
/// 
/// Performance:
/// - Composite index on (UserId, IsRead, CreatedAt) for efficient queries
/// - Supports pagination for large notification lists
/// - Automatic cleanup of old notifications (optional)
/// 
/// Relationships:
/// - Many-to-One: Notification → User (notification belongs to a user)
/// 
/// Example Usage:
/// ```csharp
/// // Create notification
/// var notification = new Notification
/// {
///     UserId = userId,
///     Type = NotificationType.RentalApproved,
///     Title = "Rental Approved",
///     Message = "Your rental request for 'Conference Room A' has been approved!",
///     RelatedEntityId = rentalId,
///     RelatedEntityType = "RoomRental",
///     ActionUrl = "/Student/Rentals",
///     IsRead = false,
///     CreatedAt = DateTime.UtcNow
/// };
/// ```
/// </remarks>
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
