using Microsoft.EntityFrameworkCore;
using CampusEvents.Models;

namespace CampusEvents.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<SavedEvent> SavedEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure SavedEvent as a many-to-many join table
        modelBuilder.Entity<SavedEvent>()
            .HasKey(se => new { se.UserId, se.EventId });

        modelBuilder.Entity<SavedEvent>()
            .HasOne(se => se.User)
            .WithMany(u => u.SavedEvents)
            .HasForeignKey(se => se.UserId);

        modelBuilder.Entity<SavedEvent>()
            .HasOne(se => se.Event)
            .WithMany(e => e.SavedByUsers)
            .HasForeignKey(se => se.EventId);

        // Configure User-Event relationship (Organizer)
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organizer)
            .WithMany(u => u.OrganizedEvents)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Event-Organization relationship
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organization)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure unique email for User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Configure unique ticket code
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.UniqueCode)
            .IsUnique();

        // Seed some initial data
        /**
        modelBuilder.Entity<Organization>().HasData(
            new Organization { Id = 5, Name = "Union", Description = "student union organization", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Organization { Id = 2, Name = "Computer Science Association", Description = "CS student group", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Organization { Id = 3, Name = "Athletics Department", Description = "Campus athletics and sports", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
/**
        modelBuilder.Entity<Ticket>().HasData(
            new Ticket { Id = 1, EventId = 1001, UserId = 501, UniqueCode = "A1B2C3D4E5", QrCodeImage = null, ClaimedAt = new DateTime(2025, 1, 10, 14, 0, 0, DateTimeKind.Utc), RedeemedAt = null, IsRedeemed = false },
            new Ticket { Id = 2, EventId = 1002, UserId = 502, UniqueCode = "F6G7H8I9J0", QrCodeImage = null, ClaimedAt = new DateTime(2025, 1, 11, 15, 30, 0, DateTimeKind.Utc), RedeemedAt = null, IsRedeemed = false },
            new Ticket { Id = 3, EventId = 1001, UserId = 503, UniqueCode = "K1L2M3N4O5", QrCodeImage = null, ClaimedAt = new DateTime(2025, 1, 12, 9, 45, 0, DateTimeKind.Utc), RedeemedAt = null, IsRedeemed = false },
            new Ticket { Id = 4, EventId = 1003, UserId = 504, UniqueCode = "P6Q7R8S9T0", QrCodeImage = null, ClaimedAt = new DateTime(2025, 1, 13, 11, 0, 0, DateTimeKind.Utc), RedeemedAt = null, IsRedeemed = true },
            new Ticket { Id = 5, EventId = 1002, UserId = 505, UniqueCode = "U1V2W3X4Y5", QrCodeImage = null, ClaimedAt = new DateTime(2025, 1, 14, 16, 15, 0, DateTimeKind.Utc), RedeemedAt = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc), IsRedeemed = true },
            new Ticket { Id = 6, EventId = 1003, UserId = 506, UniqueCode = "Z6A7B8C9D0", QrCodeImage = null, ClaimedAt = new DateTime(2025, 1, 15, 14, 30, 0, DateTimeKind.Utc), RedeemedAt = null, IsRedeemed = false },
            new Ticket { Id = 7, EventId = 1001, UserId = 507, UniqueCode = "E1F2G3H4I5", QrCodeImage = null, ClaimedAt = new DateTime(2025, 1, 16, 13, 0, 0, DateTimeKind.Utc), RedeemedAt = null, IsRedeemed = false },
            new Ticket { Id = 8, EventId = 1004, UserId = 508, UniqueCode = "J6K7L8M9N0", QrCodeImage = null, ClaimedAt = new DateTime(2025, 1, 17, 17, 45, 0, DateTimeKind.Utc), RedeemedAt = null, IsRedeemed = false },
            new Ticket { Id = 9, EventId = 1004, UserId = 509, UniqueCode = "O1P2Q3R4S5", QrCodeImage = null, ClaimedAt = new DateTime(2025, 1, 18, 18, 0, 0, DateTimeKind.Utc), RedeemedAt = null, IsRedeemed = false },
            new Ticket { Id = 10, EventId = 1002, UserId = 510, UniqueCode = "T6U7V8W9X0", QrCodeImage = null, ClaimedAt = new DateTime(2025, 1, 19, 19, 15, 0, DateTimeKind.Utc), RedeemedAt = null, IsRedeemed = false });
    **/
    }
    
}
