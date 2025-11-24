using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using System.Text;

namespace CampusEvents.Pages.Admin;

public class EventsModel : PageModel
{
    private readonly AppDbContext _context;

    public EventsModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Event> Events { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CategoryFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 10;
    public int TotalEvents { get; set; }

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        // Load events with filters
        var query = _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Organization)
            .AsQueryable();

        // Filter by approval status
        if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "All")
        {
            if (Enum.TryParse<ApprovalStatus>(StatusFilter, out var status))
            {
                query = query.Where(e => e.ApprovalStatus == status);
            }
        }

        // Filter by category
        if (!string.IsNullOrWhiteSpace(CategoryFilter) && CategoryFilter != "All")
        {
            query = query.Where(e => e.Category == CategoryFilter);
        }

        // Get total count before pagination
        TotalEvents = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(TotalEvents / (double)PageSize);

        // Ensure current page is valid
        if (CurrentPage < 1) CurrentPage = 1;
        if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

        // Apply pagination
        Events = await query
            .OrderBy(e => e.ApprovalStatus)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var evt = await _context.Events.FindAsync(id);
        if (evt == null)
        {
            Message = "Event not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        evt.ApprovalStatus = ApprovalStatus.Approved;
        await _context.SaveChangesAsync();

        Message = $"Event '{evt.Title}' has been approved and is now visible to students.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var evt = await _context.Events.FindAsync(id);
        if (evt == null)
        {
            Message = "Event not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        evt.ApprovalStatus = ApprovalStatus.Rejected;
        await _context.SaveChangesAsync();

        Message = $"Event '{evt.Title}' has been rejected.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var evt = await _context.Events
            .Include(e => e.Tickets)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evt == null)
        {
            Message = "Event not found.";
            IsSuccess = false;
            return RedirectToPage();
        }

        // Check if event has tickets issued
        if (evt.TicketsIssued > 0)
        {
            Message = $"Cannot delete event '{evt.Title}' because it has {evt.TicketsIssued} tickets issued.";
            IsSuccess = false;
            return RedirectToPage();
        }

        _context.Events.Remove(evt);
        await _context.SaveChangesAsync();

        Message = $"Event '{evt.Title}' has been deleted.";
        IsSuccess = true;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDownloadStudentsWithTicketsCSV(int id, String name)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        // Load all tickets for this event
        var tickets = await _context.Tickets
            .Include(t => t.User)
            .Where(t => t.EventId == id)
            .OrderBy(t => t.ClaimedAt)
            .ToListAsync();

        // Generate CSV content with UTF-8 BOM for better Excel compatibility
        var csv = new StringBuilder();

        // CSV Header - using shorter date format to prevent ##### in Excel
        csv.AppendLine("UserID");

        
        // CSV Rows
        foreach (var ticket in tickets)
        {
            csv.AppendLine($"{ticket.UserId}");
        }

        // Add UTF-8 BOM for Excel compatibility
        var preamble = Encoding.UTF8.GetPreamble();
        var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
        var bytesWithBOM = new byte[preamble.Length + csvBytes.Length];
        Buffer.BlockCopy(preamble, 0, bytesWithBOM, 0, preamble.Length);
        Buffer.BlockCopy(csvBytes, 0, bytesWithBOM, preamble.Length, csvBytes.Length);

        var fileName = $"{name}.csv";

        return File(bytesWithBOM, "text/csv", fileName);
    }
}
