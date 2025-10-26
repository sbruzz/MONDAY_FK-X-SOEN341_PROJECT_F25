using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

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

        Events = await query
            .OrderBy(e => e.ApprovalStatus)
            .ThenByDescending(e => e.CreatedAt)
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
}
