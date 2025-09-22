using CampusEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEvents.Services;

/// <summary>
/// Helper class for common database query operations
/// Provides reusable query patterns and filtering utilities
/// </summary>
public static class QueryHelper
{
    /// <summary>
    /// Applies pagination to an IQueryable
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Query to paginate</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated query</returns>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = Constants.Pagination.DefaultPageSize;
        if (pageSize > Constants.Pagination.MaxPageSize) pageSize = Constants.Pagination.MaxPageSize;

        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    /// <summary>
    /// Filters events by approval status
    /// </summary>
    /// <param name="query">Event query</param>
    /// <param name="status">Approval status to filter by</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<Event> FilterByApprovalStatus(this IQueryable<Event> query, ApprovalStatus? status)
    {
        if (status.HasValue)
        {
            return query.Where(e => e.ApprovalStatus == status.Value);
        }
        return query;
    }

    /// <summary>
    /// Filters events by category
    /// </summary>
    /// <param name="query">Event query</param>
    /// <param name="category">Category to filter by</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<Event> FilterByCategory(this IQueryable<Event> query, string? category)
    {
        if (!string.IsNullOrWhiteSpace(category))
        {
            return query.Where(e => e.Category.ToLower() == category.ToLower());
        }
        return query;
    }

    /// <summary>
    /// Filters events by date range
    /// </summary>
    /// <param name="query">Event query</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<Event> FilterByDateRange(this IQueryable<Event> query, DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue)
        {
            query = query.Where(e => e.EventDate >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(e => e.EventDate <= endDate.Value);
        }
        
        return query;
    }

    /// <summary>
    /// Filters events to only show future events
    /// </summary>
    /// <param name="query">Event query</param>
    /// <returns>Filtered query with only future events</returns>
    public static IQueryable<Event> FilterFutureEvents(this IQueryable<Event> query)
    {
        return query.Where(e => e.EventDate > DateTime.UtcNow);
    }

    /// <summary>
    /// Filters events to only show past events
    /// </summary>
    /// <param name="query">Event query</param>
    /// <returns>Filtered query with only past events</returns>
    public static IQueryable<Event> FilterPastEvents(this IQueryable<Event> query)
    {
        return query.Where(e => e.EventDate < DateTime.UtcNow);
    }

    /// <summary>
    /// Searches events by title or description
    /// </summary>
    /// <param name="query">Event query</param>
    /// <param name="searchTerm">Search term</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<Event> SearchEvents(this IQueryable<Event> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var term = searchTerm.ToLower();
        return query.Where(e => 
            e.Title.ToLower().Contains(term) || 
            e.Description.ToLower().Contains(term) ||
            e.Location.ToLower().Contains(term));
    }

    /// <summary>
    /// Filters events by organizer
    /// </summary>
    /// <param name="query">Event query</param>
    /// <param name="organizerId">Organizer ID to filter by</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<Event> FilterByOrganizer(this IQueryable<Event> query, int? organizerId)
    {
        if (organizerId.HasValue)
        {
            return query.Where(e => e.OrganizerId == organizerId.Value);
        }
        return query;
    }

    /// <summary>
    /// Filters events by ticket type
    /// </summary>
    /// <param name="query">Event query</param>
    /// <param name="ticketType">Ticket type to filter by</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<Event> FilterByTicketType(this IQueryable<Event> query, TicketType? ticketType)
    {
        if (ticketType.HasValue)
        {
            return query.Where(e => e.TicketType == ticketType.Value);
        }
        return query;
    }

    /// <summary>
    /// Filters events with available capacity
    /// </summary>
    /// <param name="query">Event query</param>
    /// <returns>Filtered query with events that have available tickets</returns>
    public static IQueryable<Event> FilterAvailableEvents(this IQueryable<Event> query)
    {
        return query.Where(e => e.TicketsIssued < e.Capacity);
    }

    /// <summary>
    /// Orders events by date (ascending)
    /// </summary>
    /// <param name="query">Event query</param>
    /// <returns>Ordered query</returns>
    public static IQueryable<Event> OrderByDate(this IQueryable<Event> query)
    {
        return query.OrderBy(e => e.EventDate);
    }

    /// <summary>
    /// Orders events by date (descending)
    /// </summary>
    /// <param name="query">Event query</param>
    /// <returns>Ordered query</returns>
    public static IQueryable<Event> OrderByDateDescending(this IQueryable<Event> query)
    {
        return query.OrderByDescending(e => e.EventDate);
    }

    /// <summary>
    /// Orders events by creation date (newest first)
    /// </summary>
    /// <param name="query">Event query</param>
    /// <returns>Ordered query</returns>
    public static IQueryable<Event> OrderByNewest(this IQueryable<Event> query)
    {
        return query.OrderByDescending(e => e.CreatedAt);
    }

    /// <summary>
    /// Filters users by role
    /// </summary>
    /// <param name="query">User query</param>
    /// <param name="role">Role to filter by</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<User> FilterByRole(this IQueryable<User> query, UserRole? role)
    {
        if (role.HasValue)
        {
            return query.Where(u => u.Role == role.Value);
        }
        return query;
    }

    /// <summary>
    /// Filters users by approval status
    /// </summary>
    /// <param name="query">User query</param>
    /// <param name="status">Approval status to filter by</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<User> FilterByApprovalStatus(this IQueryable<User> query, ApprovalStatus? status)
    {
        if (status.HasValue)
        {
            return query.Where(u => u.ApprovalStatus == status.Value);
        }
        return query;
    }

    /// <summary>
    /// Searches users by name or email
    /// </summary>
    /// <param name="query">User query</param>
    /// <param name="searchTerm">Search term</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<User> SearchUsers(this IQueryable<User> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var term = searchTerm.ToLower();
        return query.Where(u => 
            u.Name.ToLower().Contains(term) || 
            u.Email.ToLower().Contains(term) ||
            (u.StudentId != null && u.StudentId.Contains(term)));
    }
}

