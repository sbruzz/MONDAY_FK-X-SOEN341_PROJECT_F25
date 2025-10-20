using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using BCrypt.Net;

namespace CampusEvents.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Check if organizer/admin needs approval
        if ((user.Role == UserRole.Organizer || user.Role == UserRole.Admin)
            && user.ApprovalStatus != ApprovalStatus.Approved)
        {
            return Unauthorized(new { message = "Your account is pending approval" });
        }

        return Ok(new { user = MapUserToDto(user) });
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "Email already registered" });
        }

        // Parse role from string
        if (!Enum.TryParse<UserRole>(request.Role, out var userRole))
        {
            return BadRequest(new { message = "Invalid role selected" });
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Name = request.Name,
            Role = userRole,
            ApprovalStatus = userRole == UserRole.Student ? ApprovalStatus.Approved : ApprovalStatus.Pending
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var message = userRole == UserRole.Student 
            ? "Account created successfully!" 
            : "Account created! Awaiting admin approval.";

        return Ok(new { user = MapUserToDto(user), message });
    }

    private static object MapUserToDto(User user) => new
    {
        user.Id,
        user.Email,
        user.Name,
        Role = user.Role.ToString(),
        ApprovalStatus = user.ApprovalStatus.ToString(),
        CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
    };
}

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class SignupRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; }
}
