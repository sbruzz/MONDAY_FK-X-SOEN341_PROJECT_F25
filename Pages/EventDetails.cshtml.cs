using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Pages;

public class PublicEventDetailsModel : PageModel
{
    private readonly AppDbContext _context;

    public PublicEventDetailsModel(AppDbContext context)
    {
        _context = context;
    }

    public Event? Event { get; set; }
    public bool IsLoggedIn { get; set; }
    public string ShareUrl { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Check if user is logged in
        var userId = HttpContext.Session.GetInt32("UserId");
        IsLoggedIn = userId != null;

        // Load event with related data
        Event = await _context.Events
            .Include(e => e.Organization)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == id && e.ApprovalStatus == ApprovalStatus.Approved);

        if (Event == null)
        {
            return NotFound();
        }

        // Generate share URL
        var request = HttpContext.Request;
        ShareUrl = $"{request.Scheme}://{request.Host}/EventDetails?id={id}";

        return Page();
    }
}
