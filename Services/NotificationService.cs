using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Services;

public class NotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateNotificationAsync(
        int userId,
        NotificationType type,
        string title,
        string message,
        int? relatedEntityId = null,
        string? relatedEntityType = null,
        string? actionUrl = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return notification;
    }

    public async Task NotifyRoomDisabledAsync(int roomId)
    {
        var room = await _context.Rooms
            .Include(r => r.Rentals.Where(rental => rental.Status == RentalStatus.Approved && rental.EndTime > DateTime.UtcNow))
            .ThenInclude(rental => rental.Renter)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null) return;

        foreach (var rental in room.Rentals)
        {
            await CreateNotificationAsync(
                userId: rental.RenterId,
                type: NotificationType.RoomDisabled,
                title: "Room Disabled",
                message: $"The room '{room.Name}' for your rental on {rental.StartTime:MMM dd, yyyy} has been disabled. Please contact support.",
                relatedEntityId: rental.Id,
                relatedEntityType: "RoomRental",
                actionUrl: "/Student/Rentals"
            );
        }
    }

    public async Task NotifyRentalApprovedAsync(int rentalId)
    {
        var rental = await _context.RoomRentals
            .Include(r => r.Room)
            .Include(r => r.Renter)
            .FirstOrDefaultAsync(r => r.Id == rentalId);

        if (rental == null) return;

        await CreateNotificationAsync(
            userId: rental.RenterId,
            type: NotificationType.RentalApproved,
            title: "Rental Approved",
            message: $"Your rental request for '{rental.Room.Name}' on {rental.StartTime:MMM dd, yyyy} has been approved!",
            relatedEntityId: rentalId,
            relatedEntityType: "RoomRental",
            actionUrl: "/Student/Rentals"
        );
    }

    public async Task NotifyRentalRejectedAsync(int rentalId, string? reason = null)
    {
        var rental = await _context.RoomRentals
            .Include(r => r.Room)
            .Include(r => r.Renter)
            .FirstOrDefaultAsync(r => r.Id == rentalId);

        if (rental == null) return;

        var message = $"Your rental request for '{rental.Room.Name}' on {rental.StartTime:MMM dd, yyyy} has been rejected.";
        if (!string.IsNullOrEmpty(reason))
        {
            message += $" Reason: {reason}";
        }

        await CreateNotificationAsync(
            userId: rental.RenterId,
            type: NotificationType.RentalRejected,
            title: "Rental Rejected",
            message: message,
            relatedEntityId: rentalId,
            relatedEntityType: "RoomRental",
            actionUrl: "/Student/Rentals"
        );
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetAllNotificationsAsync(int userId, int limit = 50)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);

        // Validate ownership
        if (notification == null)
            return false;

        if (notification.UserId != userId)
            throw new UnauthorizedAccessException("Cannot mark another user's notification as read");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId, int userId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);

        // Validate ownership
        if (notification == null)
            return false;

        if (notification.UserId != userId)
            throw new UnauthorizedAccessException("Cannot delete another user's notification");

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task DeleteOldNotificationsAsync(int userId, int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && n.IsRead && n.CreatedAt < cutoffDate)
            .ToListAsync();

        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();
    }
}
