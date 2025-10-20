using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AnalyticsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("events/{eventId}")]
    public async Task<IActionResult> GetEventAnalytics(int eventId)
    {
        var analytics = await _context.EventAnalytics
            .FirstOrDefaultAsync(ea => ea.EventId == eventId);

        if (analytics == null)
        {
            // Create analytics record if it doesn't exist
            analytics = new EventAnalytics
            {
                EventId = eventId,
                ViewCount = 0,
                SaveCount = 0,
                TicketsSold = 0,
                Revenue = 0,
                AttendanceRate = 0,
                LastUpdated = DateTime.UtcNow
            };

            _context.EventAnalytics.Add(analytics);
            await _context.SaveChangesAsync();
        }

        return Ok(MapAnalyticsToDto(analytics));
    }

    [HttpPost("events/{eventId}/increment-view")]
    public async Task<IActionResult> IncrementEventView(int eventId)
    {
        var analytics = await GetOrCreateAnalytics(eventId);
        analytics.ViewCount++;
        analytics.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapAnalyticsToDto(analytics));
    }

    [HttpPost("events/{eventId}/increment-save")]
    public async Task<IActionResult> IncrementEventSave(int eventId)
    {
        var analytics = await GetOrCreateAnalytics(eventId);
        analytics.SaveCount++;
        analytics.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapAnalyticsToDto(analytics));
    }

    [HttpPost("events/{eventId}/update-ticket-sales")]
    public async Task<IActionResult> UpdateTicketSales(int eventId, [FromBody] UpdateTicketSalesRequest request)
    {
        var analytics = await GetOrCreateAnalytics(eventId);
        
        analytics.TicketsSold = request.TicketsSold;
        analytics.Revenue = request.Revenue;
        analytics.AttendanceRate = request.AttendanceRate;
        analytics.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapAnalyticsToDto(analytics));
    }

    [HttpGet("platform")]
    public async Task<IActionResult> GetPlatformAnalytics()
    {
        var totalEvents = await _context.Events.CountAsync();
        var totalUsers = await _context.Users.CountAsync();
        var totalTickets = await _context.Tickets.CountAsync();
        var activeOrganizations = await _context.Organizations.CountAsync(o => o.IsActive);

        var totalRevenue = await _context.Tickets
            .Where(t => t.PaymentStatus == PaymentStatus.Completed)
            .SumAsync(t => t.PaymentAmount);

        var eventsByCategory = await _context.Events
            .Include(e => e.CategoryEntity)
            .GroupBy(e => e.CategoryEntity!.Name)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(new
        {
            TotalEvents = totalEvents,
            TotalUsers = totalUsers,
            TotalTickets = totalTickets,
            ActiveOrganizations = activeOrganizations,
            TotalRevenue = totalRevenue,
            EventsByCategory = eventsByCategory
        });
    }

    private async Task<EventAnalytics> GetOrCreateAnalytics(int eventId)
    {
        var analytics = await _context.EventAnalytics
            .FirstOrDefaultAsync(ea => ea.EventId == eventId);

        if (analytics == null)
        {
            analytics = new EventAnalytics
            {
                EventId = eventId,
                ViewCount = 0,
                SaveCount = 0,
                TicketsSold = 0,
                Revenue = 0,
                AttendanceRate = 0,
                LastUpdated = DateTime.UtcNow
            };

            _context.EventAnalytics.Add(analytics);
            await _context.SaveChangesAsync();
        }

        return analytics;
    }

    private static object MapAnalyticsToDto(EventAnalytics analytics) => new
    {
        analytics.Id,
        analytics.EventId,
        analytics.ViewCount,
        analytics.SaveCount,
        analytics.TicketsSold,
        analytics.Revenue,
        analytics.AttendanceRate,
        LastUpdated = analytics.LastUpdated.ToString("yyyy-MM-ddTHH:mm:ssZ")
    };
}

public class UpdateTicketSalesRequest
{
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
    public decimal AttendanceRate { get; set; }
}
