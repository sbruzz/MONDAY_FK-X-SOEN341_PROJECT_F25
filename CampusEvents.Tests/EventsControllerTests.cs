using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CampusEvents.Data;
using CampusEvents.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace CampusEvents.Tests;

public class EventsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EventsControllerTests(WebApplicationFactory<Program> factory)
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
    public async Task GetEvents_ReturnsAllEvents()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var organizer = new User
        {
            Email = "organizer@example.com",
            PasswordHash = "hash",
            Name = "Organizer",
            Role = UserRole.Organizer,
            ApprovalStatus = ApprovalStatus.Approved
        };
        context.Users.Add(organizer);
        await context.SaveChangesAsync();

        var events = new List<Event>
        {
            new Event
            {
                Title = "Test Event 1",
                Description = "Description 1",
                EventDate = DateTime.UtcNow.AddDays(1),
                Location = "Location 1",
                Capacity = 100,
                TicketType = TicketType.Free,
                Price = 0,
                Category = "Academic",
                OrganizerId = organizer.Id,
                IsApproved = true
            },
            new Event
            {
                Title = "Test Event 2",
                Description = "Description 2",
                EventDate = DateTime.UtcNow.AddDays(2),
                Location = "Location 2",
                Capacity = 50,
                TicketType = TicketType.Paid,
                Price = 10,
                Category = "Social",
                OrganizerId = organizer.Id,
                IsApproved = true
            }
        };
        context.Events.AddRange(events);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/events");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement[]>(content);
        
        Assert.Equal(2, result.Length);
        Assert.Equal("Test Event 1", result[0].GetProperty("title").GetString());
        Assert.Equal("Test Event 2", result[1].GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetEvent_WithValidId_ReturnsEvent()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var organizer = new User
        {
            Email = "organizer@example.com",
            PasswordHash = "hash",
            Name = "Organizer",
            Role = UserRole.Organizer,
            ApprovalStatus = ApprovalStatus.Approved
        };
        context.Users.Add(organizer);
        await context.SaveChangesAsync();

        var eventEntity = new Event
        {
            Title = "Test Event",
            Description = "Test Description",
            EventDate = DateTime.UtcNow.AddDays(1),
            Location = "Test Location",
            Capacity = 100,
            TicketType = TicketType.Free,
            Price = 0,
            Category = "Academic",
            OrganizerId = organizer.Id,
            IsApproved = true
        };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/events/{eventEntity.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.Equal("Test Event", result.GetProperty("title").GetString());
        Assert.Equal("Test Description", result.GetProperty("description").GetString());
    }

    [Fact]
    public async Task GetEvent_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/events/999");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsOrganizer_CreatesEvent()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var organizer = new User
        {
            Email = "organizer@example.com",
            PasswordHash = "hash",
            Name = "Organizer",
            Role = UserRole.Organizer,
            ApprovalStatus = ApprovalStatus.Approved
        };
        context.Users.Add(organizer);
        await context.SaveChangesAsync();

        // Login as organizer
        var loginRequest = new { email = "organizer@example.com", password = "password" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        // Mock session for organizer
        var sessionCookie = loginResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
        if (sessionCookie != null)
        {
            _client.DefaultRequestHeaders.Add("Cookie", sessionCookie);
        }

        var createEventRequest = new
        {
            title = "New Event",
            description = "New Event Description",
            eventDate = DateTime.UtcNow.AddDays(1),
            location = "New Location",
            capacity = 100,
            ticketType = "Free",
            price = 0m,
            category = "Academic"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", createEventRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.Equal("New Event", result.GetProperty("title").GetString());
        Assert.Equal("New Event Description", result.GetProperty("description").GetString());
    }

    [Fact]
    public async Task CreateEvent_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var createEventRequest = new
        {
            title = "New Event",
            description = "New Event Description",
            eventDate = DateTime.UtcNow.AddDays(1),
            location = "New Location",
            capacity = 100,
            ticketType = "Free",
            price = 0m,
            category = "Academic"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", createEventRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEvents_WithCategoryFilter_ReturnsFilteredEvents()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var organizer = new User
        {
            Email = "organizer@example.com",
            PasswordHash = "hash",
            Name = "Organizer",
            Role = UserRole.Organizer,
            ApprovalStatus = ApprovalStatus.Approved
        };
        context.Users.Add(organizer);
        await context.SaveChangesAsync();

        var events = new List<Event>
        {
            new Event
            {
                Title = "Academic Event",
                Description = "Academic Description",
                EventDate = DateTime.UtcNow.AddDays(1),
                Location = "Academic Location",
                Capacity = 100,
                TicketType = TicketType.Free,
                Price = 0,
                Category = "Academic",
                OrganizerId = organizer.Id,
                IsApproved = true
            },
            new Event
            {
                Title = "Social Event",
                Description = "Social Description",
                EventDate = DateTime.UtcNow.AddDays(2),
                Location = "Social Location",
                Capacity = 50,
                TicketType = TicketType.Paid,
                Price = 10,
                Category = "Social",
                OrganizerId = organizer.Id,
                IsApproved = true
            }
        };
        context.Events.AddRange(events);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/events?category=Academic");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement[]>(content);
        
        Assert.Single(result);
        Assert.Equal("Academic Event", result[0].GetProperty("title").GetString());
    }
}
