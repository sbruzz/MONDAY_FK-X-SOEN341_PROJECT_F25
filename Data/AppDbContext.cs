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
    
    // Carpool and Rental system
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Passenger> Passengers { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Rental> Rentals { get; set; }

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
        
        // Configure Driver relationships
        modelBuilder.Entity<Driver>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Driver>()
            .HasOne(d => d.Event)
            .WithMany()
            .HasForeignKey(d => d.EventId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Configure Passenger relationships
        modelBuilder.Entity<Passenger>()
            .HasOne(p => p.Driver)
            .WithMany(d => d.Passengers)
            .HasForeignKey(p => p.DriverId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Passenger>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Passenger>()
            .HasOne(p => p.Event)
            .WithMany()
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Configure Rental relationships
        modelBuilder.Entity<Rental>()
            .HasOne(r => r.Room)
            .WithMany(room => room.Rentals)
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Rental>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Rental>()
            .HasOne(r => r.Event)
            .WithMany()
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.SetNull);
    }
    
}
