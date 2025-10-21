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

    // Get all events with optional filters
    [HttpGet]
    public async Task<IActionResult> GetEvents(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        // Start with all events
        var query = _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Organization)
            .AsQueryable();

        // Apply filters
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

        // Get events ordered by date
        var events = await query
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        return Ok(events.Select(MapEventToDto));
    }

    // Get single event by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Organization)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventEntity == null)
        {
            return NotFound(new { message = "Event not found" });
        }

        return Ok(MapEventToDto(eventEntity));
    }

    // Create new event (organizers and admins only)
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
    {
        // Check if user is logged in
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Must be logged in to create events" });
        }

        // Check if user is organizer or admin
        var userRole = GetCurrentUserRole();
        if (userRole != "Organizer" && userRole != "Admin")
        {
            return Forbid("Only organizers and admins can create events");
        }

        // Basic validation
        if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Description))
        {
            return BadRequest(new { message = "Title and description are required" });
        }

        if (request.EventDate < DateTime.UtcNow)
        {
            return BadRequest(new { message = "Event date must be in the future" });
        }

        if (request.Capacity <= 0)
        {
            return BadRequest(new { message = "Capacity must be greater than 0" });
        }

        // Parse ticket type
        if (!Enum.TryParse<TicketType>(request.TicketType, out var ticketType))
        {
            return BadRequest(new { message = "Invalid ticket type" });
        }

        // Create event
        var eventEntity = new Event
        {
            Title = request.Title,
            Description = request.Description,
            EventDate = request.EventDate,
            Location = request.Location,
            Capacity = request.Capacity,
            TicketType = ticketType,
            Price = request.Price,
            Category = request.Category,
            OrganizerId = userId.Value,
            OrganizationId = request.OrganizationId,
            IsApproved = userRole == "Admin" // Admin events are auto-approved
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEvent), new { id = eventEntity.Id }, MapEventToDto(eventEntity));
    }

    // Update event (organizer or admin only)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
        {
            return NotFound(new { message = "Event not found" });
        }

        // Check if user can edit this event
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        
        if (userId == null || (eventEntity.OrganizerId != userId.Value && userRole != "Admin"))
        {
            return Forbid("You can only edit your own events");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Title))
            eventEntity.Title = request.Title;
        
        if (!string.IsNullOrEmpty(request.Description))
            eventEntity.Description = request.Description;
        
        if (request.EventDate.HasValue)
            eventEntity.EventDate = request.EventDate.Value;
        
        if (!string.IsNullOrEmpty(request.Location))
            eventEntity.Location = request.Location;
        
        if (request.Capacity.HasValue)
            eventEntity.Capacity = request.Capacity.Value;
        
        if (!string.IsNullOrEmpty(request.TicketType) && Enum.TryParse<TicketType>(request.TicketType, out var ticketType))
            eventEntity.TicketType = ticketType;
        
        if (request.Price.HasValue)
            eventEntity.Price = request.Price.Value;
        
        if (!string.IsNullOrEmpty(request.Category))
            eventEntity.Category = request.Category;
        
        if (request.OrganizationId.HasValue)
            eventEntity.OrganizationId = request.OrganizationId.Value;

        await _context.SaveChangesAsync();

        return Ok(MapEventToDto(eventEntity));
    }

    // Delete event (organizer or admin only)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
        {
            return NotFound(new { message = "Event not found" });
        }

        // Check if user can delete this event
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        
        if (userId == null || (eventEntity.OrganizerId != userId.Value && userRole != "Admin"))
        {
            return Forbid("You can only delete your own events");
        }

        _context.Events.Remove(eventEntity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Helper methods for authentication
    private int? GetCurrentUserId()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (int.TryParse(userIdString, out var userId))
        {
            return userId;
        }
        return null;
    }

    private string? GetCurrentUserRole()
    {
        return HttpContext.Session.GetString("UserRole");
    }

    // Simple mapping to return event data
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

// Simple request classes
public class CreateEventRequest
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DateTime EventDate { get; set; }
    public required string Location { get; set; }
    public required int Capacity { get; set; }
    public required string TicketType { get; set; }
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
    public string? TicketType { get; set; }
    public decimal? Price { get; set; }
    public string? Category { get; set; }
    public int? OrganizationId { get; set; }
}

