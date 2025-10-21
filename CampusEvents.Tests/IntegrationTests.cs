using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CampusEvents.Data;
using CampusEvents.Models;
using System.Net.Http.Json;
using System.Text.Json;
using BCrypt.Net;

namespace CampusEvents.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("IntegrationTestDb");
                });
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CompleteUserWorkflow_StudentSignupToEventCreation()
    {
        // Step 1: Student signs up
        var signupRequest = new
        {
            email = "student@example.com",
            password = "password123",
            name = "Test Student",
            role = "Student"
        };

        var signupResponse = await _client.PostAsJsonAsync("/api/auth/signup", signupRequest);
        signupResponse.EnsureSuccessStatusCode();
        
        var signupContent = await signupResponse.Content.ReadAsStringAsync();
        var signupResult = JsonSerializer.Deserialize<JsonElement>(signupContent);
        Assert.Equal("student@example.com", signupResult.GetProperty("user").GetProperty("email").GetString());

        // Step 2: Student logs in
        var loginRequest = new
        {
            email = "student@example.com",
            password = "password123"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        // Step 3: Student views events
        var eventsResponse = await _client.GetAsync("/api/events");
        eventsResponse.EnsureSuccessStatusCode();
        
        var eventsContent = await eventsResponse.Content.ReadAsStringAsync();
        var events = JsonSerializer.Deserialize<JsonElement[]>(eventsContent);
        Assert.NotNull(events);

        // Step 4: Create an organizer account
        var organizerSignupRequest = new
        {
            email = "organizer@example.com",
            password = "password123",
            name = "Test Organizer",
            role = "Organizer"
        };

        var organizerSignupResponse = await _client.PostAsJsonAsync("/api/auth/signup", organizerSignupRequest);
        organizerSignupResponse.EnsureSuccessStatusCode();

        // Step 5: Approve organizer (simulate admin action)
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var organizer = await context.Users.FirstAsync(u => u.Email == "organizer@example.com");
        organizer.ApprovalStatus = ApprovalStatus.Approved;
        await context.SaveChangesAsync();

        // Step 6: Organizer logs in
        var organizerLoginRequest = new
        {
            email = "organizer@example.com",
            password = "password123"
        };

        var organizerLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", organizerLoginRequest);
        organizerLoginResponse.EnsureSuccessStatusCode();

        // Step 7: Organizer creates an event
        var createEventRequest = new
        {
            title = "Integration Test Event",
            description = "This event was created during integration testing",
            eventDate = DateTime.UtcNow.AddDays(7),
            location = "Test Location",
            capacity = 50,
            ticketType = "Free",
            price = 0m,
            category = "Academic"
        };

        var createEventResponse = await _client.PostAsJsonAsync("/api/events", createEventRequest);
        createEventResponse.EnsureSuccessStatusCode();
        
        var eventContent = await createEventResponse.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<JsonElement>(eventContent);
        Assert.Equal("Integration Test Event", createdEvent.GetProperty("title").GetString());

        // Step 8: Verify event appears in events list
        var updatedEventsResponse = await _client.GetAsync("/api/events");
        updatedEventsResponse.EnsureSuccessStatusCode();
        
        var updatedEventsContent = await updatedEventsResponse.Content.ReadAsStringAsync();
        var updatedEvents = JsonSerializer.Deserialize<JsonElement[]>(updatedEventsContent);
        
        var testEvent = updatedEvents.FirstOrDefault(e => e.GetProperty("title").GetString() == "Integration Test Event");
        Assert.NotNull(testEvent);
    }

    [Fact]
    public async Task EventFilteringAndSearch_WorksCorrectly()
    {
        // Arrange - Create test data
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var organizer = new User
        {
            Email = "organizer@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Name = "Test Organizer",
            Role = UserRole.Organizer,
            ApprovalStatus = ApprovalStatus.Approved
        };
        context.Users.Add(organizer);
        await context.SaveChangesAsync();

        var events = new List<Event>
        {
            new Event
            {
                Title = "Computer Science Workshop",
                Description = "Learn programming basics",
                EventDate = DateTime.UtcNow.AddDays(1),
                Location = "CS Lab",
                Capacity = 30,
                TicketType = TicketType.Free,
                Price = 0,
                Category = "Academic",
                OrganizerId = organizer.Id,
                IsApproved = true
            },
            new Event
            {
                Title = "Sports Tournament",
                Description = "Basketball championship",
                EventDate = DateTime.UtcNow.AddDays(2),
                Location = "Gymnasium",
                Capacity = 100,
                TicketType = TicketType.Paid,
                Price = 5,
                Category = "Sports",
                OrganizerId = organizer.Id,
                IsApproved = true
            },
            new Event
            {
                Title = "Art Exhibition",
                Description = "Student artwork showcase",
                EventDate = DateTime.UtcNow.AddDays(3),
                Location = "Art Gallery",
                Capacity = 50,
                TicketType = TicketType.Free,
                Price = 0,
                Category = "Arts",
                OrganizerId = organizer.Id,
                IsApproved = true
            }
        };
        context.Events.AddRange(events);
        await context.SaveChangesAsync();

        // Test 1: Get all events
        var allEventsResponse = await _client.GetAsync("/api/events");
        allEventsResponse.EnsureSuccessStatusCode();
        var allEventsContent = await allEventsResponse.Content.ReadAsStringAsync();
        var allEvents = JsonSerializer.Deserialize<JsonElement[]>(allEventsContent);
        Assert.Equal(3, allEvents.Length);

        // Test 2: Filter by category
        var academicEventsResponse = await _client.GetAsync("/api/events?category=Academic");
        academicEventsResponse.EnsureSuccessStatusCode();
        var academicEventsContent = await academicEventsResponse.Content.ReadAsStringAsync();
        var academicEvents = JsonSerializer.Deserialize<JsonElement[]>(academicEventsContent);
        Assert.Single(academicEvents);
        Assert.Equal("Computer Science Workshop", academicEvents[0].GetProperty("title").GetString());

        // Test 3: Search by title
        var searchResponse = await _client.GetAsync("/api/events?search=Sports");
        searchResponse.EnsureSuccessStatusCode();
        var searchContent = await searchResponse.Content.ReadAsStringAsync();
        var searchResults = JsonSerializer.Deserialize<JsonElement[]>(searchContent);
        Assert.Single(searchResults);
        Assert.Equal("Sports Tournament", searchResults[0].GetProperty("title").GetString());

        // Test 4: Filter by date range
        var tomorrow = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
        var dayAfterTomorrow = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd");
        var dateFilterResponse = await _client.GetAsync($"/api/events?dateFrom={tomorrow}&dateTo={dayAfterTomorrow}");
        dateFilterResponse.EnsureSuccessStatusCode();
        var dateFilterContent = await dateFilterResponse.Content.ReadAsStringAsync();
        var dateFilterResults = JsonSerializer.Deserialize<JsonElement[]>(dateFilterContent);
        Assert.Equal(2, dateFilterResults.Length);
    }

    [Fact]
    public async Task AuthenticationFlow_WithSessionManagement()
    {
        // Step 1: Sign up a user
        var signupRequest = new
        {
            email = "testuser@example.com",
            password = "password123",
            name = "Test User",
            role = "Student"
        };

        var signupResponse = await _client.PostAsJsonAsync("/api/auth/signup", signupRequest);
        signupResponse.EnsureSuccessStatusCode();

        // Step 2: Login and establish session
        var loginRequest = new
        {
            email = "testuser@example.com",
            password = "password123"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        // Extract session cookie
        var cookies = loginResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
        if (cookies != null)
        {
            _client.DefaultRequestHeaders.Add("Cookie", cookies);
        }

        // Step 3: Access protected endpoint (get current user)
        var currentUserResponse = await _client.GetAsync("/api/auth/current");
        currentUserResponse.EnsureSuccessStatusCode();
        
        var currentUserContent = await currentUserResponse.Content.ReadAsStringAsync();
        var currentUser = JsonSerializer.Deserialize<JsonElement>(currentUserContent);
        Assert.Equal("testuser@example.com", currentUser.GetProperty("email").GetString());

        // Step 4: Logout
        var logoutResponse = await _client.PostAsync("/api/auth/logout", null);
        logoutResponse.EnsureSuccessStatusCode();

        // Step 5: Try to access protected endpoint after logout
        var protectedResponse = await _client.GetAsync("/api/auth/current");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, protectedResponse.StatusCode);
    }

    [Fact]
    public async Task EventCRUD_Operations_WorkCorrectly()
    {
        // Arrange - Create organizer
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var organizer = new User
        {
            Email = "organizer@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Name = "Test Organizer",
            Role = UserRole.Organizer,
            ApprovalStatus = ApprovalStatus.Approved
        };
        context.Users.Add(organizer);
        await context.SaveChangesAsync();

        // Login as organizer
        var loginRequest = new { email = "organizer@example.com", password = "password123" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        // Extract session cookie
        var cookies = loginResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
        if (cookies != null)
        {
            _client.DefaultRequestHeaders.Add("Cookie", cookies);
        }

        // Step 1: Create event
        var createEventRequest = new
        {
            title = "CRUD Test Event",
            description = "Testing CRUD operations",
            eventDate = DateTime.UtcNow.AddDays(5),
            location = "Test Location",
            capacity = 75,
            ticketType = "Paid",
            price = 15m,
            category = "Workshop"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/events", createEventRequest);
        createResponse.EnsureSuccessStatusCode();
        
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<JsonElement>(createContent);
        var eventId = createdEvent.GetProperty("id").GetInt32();

        // Step 2: Read event
        var readResponse = await _client.GetAsync($"/api/events/{eventId}");
        readResponse.EnsureSuccessStatusCode();
        
        var readContent = await readResponse.Content.ReadAsStringAsync();
        var readEvent = JsonSerializer.Deserialize<JsonElement>(readContent);
        Assert.Equal("CRUD Test Event", readEvent.GetProperty("title").GetString());

        // Step 3: Update event
        var updateEventRequest = new
        {
            title = "Updated CRUD Test Event",
            description = "Updated description",
            capacity = 100
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/events/{eventId}", updateEventRequest);
        updateResponse.EnsureSuccessStatusCode();
        
        var updateContent = await updateResponse.Content.ReadAsStringAsync();
        var updatedEvent = JsonSerializer.Deserialize<JsonElement>(updateContent);
        Assert.Equal("Updated CRUD Test Event", updatedEvent.GetProperty("title").GetString());
        Assert.Equal(100, updatedEvent.GetProperty("capacity").GetInt32());

        // Step 4: Delete event
        var deleteResponse = await _client.DeleteAsync($"/api/events/{eventId}");
        deleteResponse.EnsureSuccessStatusCode();

        // Step 5: Verify deletion
        var verifyDeleteResponse = await _client.GetAsync($"/api/events/{eventId}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);
    }
}
