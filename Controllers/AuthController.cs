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

    // Simple login endpoint - checks email/password and sets session
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Basic validation
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        // Find user by email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        // Check password
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

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Set session (simple session-based auth)
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserRole", user.Role.ToString());

        return Ok(new { user = MapUserToDto(user) });
    }

    // Simple signup endpoint - creates new user
    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        // Basic validation
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) || 
            string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Role))
        {
            return BadRequest(new { message = "All fields are required" });
        }

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

    // Simple logout - clears session
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out successfully" });
    }

    // Get current user from session
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        return Ok(MapUserToDto(user));
    }

    // Helper method to get current user ID from session
    private int? GetCurrentUserId()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (int.TryParse(userIdString, out var userId))
        {
            return userId;
        }
        return null;
    }

    // Helper method to check if current user is admin
    private bool IsAdmin()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return role == "Admin";
    }

    // Simple mapping to return user data
    private static object MapUserToDto(User user) => new
    {
        user.Id,
        user.Email,
        user.Name,
        Role = user.Role.ToString(),
        ApprovalStatus = user.ApprovalStatus.ToString(),
        CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        LastLoginAt = user.LastLoginAt?.ToString("yyyy-MM-ddTHH:mm:ssZ")
    };
}

// Simple request classes
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

