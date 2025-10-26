using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEvents.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        // Clear the session
        HttpContext.Session.Clear();

        // Redirect to login page
        return RedirectToPage("/Index");
    }
}
