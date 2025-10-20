namespace CampusEvents.Models;

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}

public enum NotificationType
{
    EventReminder,
    EventUpdate,
    TicketClaimed,
    EventApproved,
    SystemAlert
}

public enum ScanMethod
{
    WebCamera,
    FileUpload,
    Manual
}

public enum OrganizationRole
{
    Member,
    Manager,
    Admin
}
