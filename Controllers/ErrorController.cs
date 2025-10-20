using Microsoft.AspNetCore.Mvc;

namespace CampusEvents.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ErrorController : ControllerBase
{
    [HttpGet]
    public IActionResult Error()
    {
        return Problem("An error occurred while processing your request.");
    }
}
