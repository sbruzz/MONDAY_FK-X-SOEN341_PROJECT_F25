using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;

namespace CampusEvents.Pages.Admin;

public class AdminHomeModel : PageModel
{
    private readonly AppDbContext _context;

    public AdminHomeModel(AppDbContext context)
    {
        _context = context;
    }

    public string UserName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            UserName = user.UserName;
        }

        return Page();
    }
}
