namespace CampusEvents.Models.Requests;

public class MarkNotificationReadRequest
{
    public int Id { get; set; }
}

public class MarkAllNotificationsReadRequest
{
    // UserId will come from session, no properties needed
}
