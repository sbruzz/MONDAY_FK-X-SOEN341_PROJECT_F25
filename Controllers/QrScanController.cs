using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QrScanController : ControllerBase
{
    private readonly AppDbContext _context;

    public QrScanController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateTicket([FromBody] ValidateTicketRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var ticket = await _context.Tickets
            .Include(t => t.Event)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UniqueCode == request.TicketCode);

        if (ticket == null)
        {
            // Log invalid scan attempt
            await LogQrScan(null, userId.Value, request.ScanMethod, request.ScanLocation, false);
            return Ok(new { Valid = false, Message = "Invalid ticket code" });
        }

        // Check if ticket is already redeemed
        if (ticket.IsRedeemed)
        {
            await LogQrScan(ticket.Id, userId.Value, request.ScanMethod, request.ScanLocation, false);
            return Ok(new { Valid = false, Message = "Ticket already redeemed" });
        }

        // Check if event date has passed
        if (ticket.Event.EventDate < DateTime.UtcNow)
        {
            await LogQrScan(ticket.Id, userId.Value, request.ScanMethod, request.ScanLocation, false);
            return Ok(new { Valid = false, Message = "Event has already occurred" });
        }

        // Mark ticket as redeemed
        ticket.IsRedeemed = true;
        ticket.RedeemedAt = DateTime.UtcNow;

        // Log successful scan
        await LogQrScan(ticket.Id, userId.Value, request.ScanMethod, request.ScanLocation, true);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Valid = true,
            Message = "Ticket validated successfully",
            Ticket = new
            {
                ticket.Id,
                ticket.UniqueCode,
                Event = new
                {
                    ticket.Event.Id,
                    ticket.Event.Title,
                    ticket.Event.EventDate,
                    ticket.Event.Location
                },
                User = new
                {
                    ticket.User.Id,
                    ticket.User.Name,
                    ticket.User.Email
                },
                RedeemedAt = ticket.RedeemedAt
            }
        });
    }

    [HttpGet("tickets/{ticketId}/scan-history")]
    public async Task<IActionResult> GetTicketScanHistory(int ticketId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // Check if user has permission to view this ticket's scan history
        var ticket = await _context.Tickets
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
        {
            return NotFound();
        }

        // Allow organizer, admin, or ticket owner to view scan history
        var hasPermission = ticket.UserId == userId.Value ||
                           ticket.Event.OrganizerId == userId.Value ||
                           IsAdmin(userId.Value);

        if (!hasPermission)
        {
            return Unauthorized();
        }

        var scanLogs = await _context.QrScanLogs
            .Include(qsl => qsl.Scanner)
            .Where(qsl => qsl.TicketId == ticketId)
            .OrderByDescending(qsl => qsl.ScannedAt)
            .ToListAsync();

        return Ok(scanLogs.Select(MapScanLogToDto));
    }

    [HttpGet("events/{eventId}/scan-summary")]
    public async Task<IActionResult> GetEventScanSummary(int eventId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // Check if user is organizer or admin
        var eventEntity = await _context.Events.FindAsync(eventId);
        if (eventEntity == null)
        {
            return NotFound();
        }

        var hasPermission = eventEntity.OrganizerId == userId.Value || IsAdmin(userId.Value);
        if (!hasPermission)
        {
            return Unauthorized();
        }

        var tickets = await _context.Tickets
            .Where(t => t.EventId == eventId)
            .ToListAsync();

        var totalTickets = tickets.Count;
        var redeemedTickets = tickets.Count(t => t.IsRedeemed);
        var pendingTickets = totalTickets - redeemedTickets;

        var scanLogs = await _context.QrScanLogs
            .Include(qsl => qsl.Scanner)
            .Where(qsl => qsl.TicketId != null && tickets.Select(t => t.Id).Contains(qsl.TicketId))
            .OrderByDescending(qsl => qsl.ScannedAt)
            .ToListAsync();

        var recentScans = scanLogs.Take(10).Select(MapScanLogToDto);

        return Ok(new
        {
            TotalTickets = totalTickets,
            RedeemedTickets = redeemedTickets,
            PendingTickets = pendingTickets,
            RedemptionRate = totalTickets > 0 ? (decimal)redeemedTickets / totalTickets * 100 : 0,
            RecentScans = recentScans
        });
    }

    private async Task LogQrScan(int? ticketId, int scannedBy, ScanMethod scanMethod, string? scanLocation, bool isValid)
    {
        var scanLog = new QrScanLog
        {
            TicketId = ticketId ?? 0, // Will be ignored if ticketId is null
            ScannedBy = scannedBy,
            ScanMethod = scanMethod,
            ScanLocation = scanLocation,
            ScannedAt = DateTime.UtcNow,
            IsValid = isValid
        };

        _context.QrScanLogs.Add(scanLog);
        await _context.SaveChangesAsync();
    }

    private int? GetCurrentUserId()
    {
        // TODO: Implement proper session/auth check
        return null;
    }

    private bool IsAdmin(int userId)
    {
        // TODO: Implement admin check
        return false;
    }

    private static object MapScanLogToDto(QrScanLog scanLog) => new
    {
        scanLog.Id,
        scanLog.TicketId,
        ScannedBy = new
        {
            scanLog.Scanner.Id,
            scanLog.Scanner.Name,
            scanLog.Scanner.Email
        },
        ScanMethod = scanLog.ScanMethod.ToString(),
        scanLog.ScanLocation,
        ScannedAt = scanLog.ScannedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        scanLog.IsValid
    };
}

public class ValidateTicketRequest
{
    public required string TicketCode { get; set; }
    public ScanMethod ScanMethod { get; set; }
    public string? ScanLocation { get; set; }
}
