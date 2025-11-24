using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CampusEvents.Services;
using CampusEvents.Models;
using CampusEvents.Models.Requests;
using System.Text.Json;

namespace CampusEvents.Pages.Shared;

public class GetNotificationsModel : PageModel
{
    private readonly NotificationService _notificationService;

    public GetNotificationsModel(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return new JsonResult(new { error = "Not authenticated" }) { StatusCode = 401 };

        var notifications = await _notificationService.GetAllNotificationsAsync(userId.Value, limit: 20);
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId.Value);

        return new JsonResult(new
        {
            notifications = notifications.Select(n => new
            {
                id = n.Id,
                title = n.Title,
                message = n.Message,
                type = n.Type.ToString(),
                isRead = n.IsRead,
                createdAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                actionUrl = n.ActionUrl
            }),
            unreadCount
        });
    }

    public async Task<IActionResult> OnPostMarkReadAsync([FromBody] MarkNotificationReadRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return new JsonResult(new { error = "Not authenticated" }) { StatusCode = 401 };

        try
        {
            var success = await _notificationService.MarkAsReadAsync(request.Id, userId.Value);
            if (!success)
                return new JsonResult(new { error = "Notification not found" }) { StatusCode = 404 };

            return new JsonResult(new { success = true });
        }
        catch (UnauthorizedAccessException)
        {
            return new JsonResult(new { error = "Access denied" }) { StatusCode = 403 };
        }
    }

    public async Task<IActionResult> OnPostMarkAllReadAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return new JsonResult(new { error = "Not authenticated" }) { StatusCode = 401 };

        await _notificationService.MarkAllAsReadAsync(userId.Value);
        return new JsonResult(new { success = true });
    }
}
