using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CampusEvents.Data;
using CampusEvents.Models;
using System.Net.Http.Json;
using System.Text.Json;
using BCrypt.Net;

namespace CampusEvents.Tests;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Add in-memory database
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Create a test user
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Name = "Test User",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var loginRequest = new
        {
            email = "test@example.com",
            password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(result.TryGetProperty("user", out var userElement));
        Assert.Equal("test@example.com", userElement.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new
        {
            email = "nonexistent@example.com",
            password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Signup_WithValidData_CreatesUser()
    {
        // Arrange
        var signupRequest = new
        {
            email = "newuser@example.com",
            password = "password123",
            name = "New User",
            role = "Student"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/signup", signupRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(result.TryGetProperty("user", out var userElement));
        Assert.Equal("newuser@example.com", userElement.GetProperty("email").GetString());
        Assert.Equal("New User", userElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Signup_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var existingUser = new User
        {
            Email = "existing@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Name = "Existing User",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var signupRequest = new
        {
            email = "existing@example.com",
            password = "password123",
            name = "New User",
            role = "Student"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/signup", signupRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ClearsSession()
    {
        // Arrange - First login to establish session
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Name = "Test User",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var loginRequest = new { email = "test@example.com", password = "password123" };
        await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Act
        var response = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
