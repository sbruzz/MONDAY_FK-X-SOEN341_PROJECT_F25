using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EventsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var query = _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Organization)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(e => e.Category == category);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e => e.Title.Contains(search) || e.Description.Contains(search));
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(e => e.EventDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(e => e.EventDate <= dateTo.Value);
        }

        var events = await query
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        return Ok(events.Select(MapEventToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Organization)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventEntity == null)
        {
            return NotFound();
        }

        return Ok(MapEventToDto(eventEntity));
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
    {
        // TODO: Get current user from session/auth
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var eventEntity = new Event
        {
            Title = request.Title,
            Description = request.Description,
            EventDate = request.EventDate,
            Location = request.Location,
            Capacity = request.Capacity,
            TicketType = request.TicketType,
            Price = request.Price,
            Category = request.Category,
            OrganizerId = userId.Value,
            OrganizationId = request.OrganizationId,
            IsApproved = false // Events need approval
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEvent), new { id = eventEntity.Id }, MapEventToDto(eventEntity));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
        {
            return NotFound();
        }

        // TODO: Check if user is organizer or admin
        var userId = GetCurrentUserId();
        if (userId == null || (eventEntity.OrganizerId != userId && !IsAdmin(userId)))
        {
            return Unauthorized();
        }

        eventEntity.Title = request.Title ?? eventEntity.Title;
        eventEntity.Description = request.Description ?? eventEntity.Description;
        eventEntity.EventDate = request.EventDate ?? eventEntity.EventDate;
        eventEntity.Location = request.Location ?? eventEntity.Location;
        eventEntity.Capacity = request.Capacity ?? eventEntity.Capacity;
        eventEntity.TicketType = request.TicketType ?? eventEntity.TicketType;
        eventEntity.Price = request.Price ?? eventEntity.Price;
        eventEntity.Category = request.Category ?? eventEntity.Category;
        eventEntity.OrganizationId = request.OrganizationId ?? eventEntity.OrganizationId;

        await _context.SaveChangesAsync();

        return Ok(MapEventToDto(eventEntity));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
        {
            return NotFound();
        }

        // TODO: Check if user is organizer or admin
        var userId = GetCurrentUserId();
        if (userId == null || (eventEntity.OrganizerId != userId && !IsAdmin(userId)))
        {
            return Unauthorized();
        }

        _context.Events.Remove(eventEntity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int? GetCurrentUserId()
    {
        // TODO: Implement proper session/auth check
        // For now, return null - this should be replaced with actual auth
        return null;
    }

    private bool IsAdmin(int userId)
    {
        // TODO: Implement admin check
        return false;
    }

    private static object MapEventToDto(Event eventEntity) => new
    {
        eventEntity.Id,
        eventEntity.Title,
        eventEntity.Description,
        EventDate = eventEntity.EventDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        eventEntity.Location,
        eventEntity.Capacity,
        eventEntity.TicketsIssued,
        TicketType = eventEntity.TicketType.ToString(),
        eventEntity.Price,
        eventEntity.Category,
        eventEntity.OrganizerId,
        eventEntity.OrganizationId,
        eventEntity.IsApproved,
        CreatedAt = eventEntity.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        Organizer = eventEntity.Organizer != null ? new
        {
            eventEntity.Organizer.Id,
            eventEntity.Organizer.Name,
            eventEntity.Organizer.Email
        } : null,
        Organization = eventEntity.Organization != null ? new
        {
            eventEntity.Organization.Id,
            eventEntity.Organization.Name,
            eventEntity.Organization.Description
        } : null
    };
}

public class CreateEventRequest
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DateTime EventDate { get; set; }
    public required string Location { get; set; }
    public required int Capacity { get; set; }
    public required TicketType TicketType { get; set; }
    public required decimal Price { get; set; }
    public required string Category { get; set; }
    public int? OrganizationId { get; set; }
}

public class UpdateEventRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? EventDate { get; set; }
    public string? Location { get; set; }
    public int? Capacity { get; set; }
    public TicketType? TicketType { get; set; }
    public decimal? Price { get; set; }
    public string? Category { get; set; }
    public int? OrganizationId { get; set; }
}
