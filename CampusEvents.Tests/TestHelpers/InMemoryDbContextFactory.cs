using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Tests.TestHelpers;

/// <summary>
/// Factory for creating in-memory database contexts for unit tests
/// Uses InMemory provider for fast, isolated tests
/// </summary>
public static class InMemoryDbContextFactory
{
    /// <summary>
    /// Creates a new in-memory database context for testing
    /// </summary>
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates a context with seed data for common test scenarios
    /// </summary>
    public static AppDbContext CreateWithSeedData()
    {
        var context = Create();
        SeedTestData(context);
        return context;
    }

    /// <summary>
    /// Seeds common test data (users, events, etc.)
    /// </summary>
    public static void SeedTestData(AppDbContext context)
    {
        // Seed users
        var student = new User
        {
            Id = 1,
            Email = "student@test.com",
            PasswordHash = "hashed",
            Name = "Test Student",
            Role = UserRole.Student,
            ApprovalStatus = ApprovalStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };

        var organizer = new User
        {
            Id = 2,
            Email = "organizer@test.com",
            PasswordHash = "hashed",
            Name = "Test Organizer",
            Role = UserRole.Organizer,
            ApprovalStatus = ApprovalStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };

        var admin = new User
        {
            Id = 3,
            Email = "admin@test.com",
            PasswordHash = "hashed",
            Name = "Test Admin",
            Role = UserRole.Admin,
            ApprovalStatus = ApprovalStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(student, organizer, admin);

        // Seed event
        var testEvent = new Event
        {
            Id = 1,
            Title = "Test Event",
            Description = "Test Description",
            EventDate = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Capacity = 100,
            TicketType = TicketType.Free,
            Price = 0,
            Category = EventCategory.Other,
            OrganizerId = 2,
            ApprovalStatus = ApprovalStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };

        context.Events.Add(testEvent);
        context.SaveChanges();
    }

    /// <summary>
    /// Disposes the context and cleans up
    /// </summary>
    public static void Dispose(AppDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}

