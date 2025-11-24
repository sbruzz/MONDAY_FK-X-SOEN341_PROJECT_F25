using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using CampusEvents.Data;
using CampusEvents.Models;

namespace CampusEvents.Tests.TestHelpers;

/// <summary>
/// Factory for creating SQLite in-memory database contexts for integration tests
/// Uses SQLite in-memory for more realistic database behavior
/// </summary>
public static class IntegrationTestDbContextFactory
{
    /// <summary>
    /// Creates a new SQLite in-memory database context for integration testing
    /// </summary>
    public static AppDbContext Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates a context with seed data for integration test scenarios
    /// </summary>
    public static AppDbContext CreateWithSeedData()
    {
        var context = Create();
        SeedTestData(context);
        return context;
    }

    /// <summary>
    /// Seeds common test data for integration tests
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
            Category = "Test",
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

