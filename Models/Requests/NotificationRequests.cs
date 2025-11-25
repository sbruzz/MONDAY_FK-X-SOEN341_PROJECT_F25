namespace CampusEvents.Models.Requests;

/// <summary>
/// Request model for marking a single notification as read.
/// Used in API endpoints and page models for notification management.
/// </summary>
/// <remarks>
/// This class represents the request payload for marking a specific notification
/// as read. The notification ID is provided in the request, and the user ID
/// is typically extracted from the session for security purposes.
/// 
/// Usage:
/// - Sent from client when user clicks "Mark as Read" on a notification
/// - Processed by NotificationService.MarkAsReadAsync()
/// - User ID validated to ensure user can only mark their own notifications
/// </remarks>
public class MarkNotificationReadRequest
{
    /// <summary>
    /// ID of the notification to mark as read.
    /// Must correspond to a notification owned by the current user.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Request model for marking all user's notifications as read.
/// Used in API endpoints and page models for bulk notification management.
/// </summary>
/// <remarks>
/// This class represents the request payload for marking all unread notifications
/// for the current user as read. No properties are needed as the user ID is
/// extracted from the session.
/// 
/// Usage:
/// - Sent from client when user clicks "Mark All as Read"
/// - Processed by NotificationService.MarkAllAsReadAsync()
/// - User ID extracted from session/authentication context
/// 
/// Security:
/// - User ID comes from session, not request body
/// - Prevents users from marking other users' notifications as read
/// </remarks>
public class MarkAllNotificationsReadRequest
{
    // UserId will come from session, no properties needed
    // This ensures users can only mark their own notifications as read
}
