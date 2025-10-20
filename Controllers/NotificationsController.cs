using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserNotifications([FromQuery] bool? unreadOnly = false)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = _context.Notifications
            .Where(n => n.UserId == userId.Value)
            .Include(n => n.Event)
            .OrderByDescending(n => n.CreatedAt) as IQueryable<Notification>;

        if (unreadOnly == true)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query.ToListAsync();

        return Ok(notifications.Select(MapNotificationToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotification(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var notification = await _context.Notifications
            .Include(n => n.Event)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId.Value);

        if (notification == null)
        {
            return NotFound();
        }

        return Ok(MapNotificationToDto(notification));
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
    {
        // TODO: Check if user is admin or organizer
        var userId = GetCurrentUserId();
        if (userId == null || !IsAdminOrOrganizer(userId.Value))
        {
            return Unauthorized();
        }

        var notification = new Notification
        {
            UserId = request.UserId,
            EventId = request.EventId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, MapNotificationToDto(notification));
    }

    [HttpPost("{id}/mark-read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId.Value);

        if (notification == null)
        {
            return NotFound();
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return Ok(MapNotificationToDto(notification));
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId.Value && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new { Message = "All notifications marked as read" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId.Value);

        if (notification == null)
        {
            return NotFound();
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("send-event-reminder")]
    public async Task<IActionResult> SendEventReminder([FromBody] SendEventReminderRequest request)
    {
        // TODO: Check if user is admin or organizer
        var userId = GetCurrentUserId();
        if (userId == null || !IsAdminOrOrganizer(userId.Value))
        {
            return Unauthorized();
        }

        var eventEntity = await _context.Events.FindAsync(request.EventId);
        if (eventEntity == null)
        {
            return NotFound("Event not found");
        }

        // Get all users who have saved this event
        var savedUsers = await _context.SavedEvents
            .Where(se => se.EventId == request.EventId)
            .Select(se => se.UserId)
            .ToListAsync();

        var notifications = savedUsers.Select(userId => new Notification
        {
            UserId = userId,
            EventId = request.EventId,
            Type = NotificationType.EventReminder,
            Title = $"Reminder: {eventEntity.Title}",
            Message = request.Message ?? $"Don't forget! {eventEntity.Title} is happening soon.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Event reminder sent to {notifications.Count} users" });
    }

    private int? GetCurrentUserId()
    {
        // TODO: Implement proper session/auth check
        return null;
    }

    private bool IsAdminOrOrganizer(int userId)
    {
        // TODO: Implement admin/organizer check
        return false;
    }

    private static object MapNotificationToDto(Notification notification) => new
    {
        notification.Id,
        notification.UserId,
        notification.EventId,
        Type = notification.Type.ToString(),
        notification.Title,
        notification.Message,
        notification.IsRead,
        CreatedAt = notification.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        Event = notification.Event != null ? new
        {
            notification.Event.Id,
            notification.Event.Title,
            notification.Event.EventDate
        } : null
    };
}

public class CreateNotificationRequest
{
    public int UserId { get; set; }
    public int? EventId { get; set; }
    public NotificationType Type { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
}

public class SendEventReminderRequest
{
    public int EventId { get; set; }
    public string? Message { get; set; }
}
