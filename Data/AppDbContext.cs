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
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<Drivers> Drivers{ get; set; }
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
        
        //configure rental 
        modelBuilder.Entity<Rental>()
            .HasKey(r => new { r.UserId, r.EventId });

        modelBuilder.Entity<Rental>()
            .HasMany(r => r.users)
            .WithMany(u => u.Rentedrooms)
            .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<Rental>()
            .HasOne(r => r.Event)
            .WithOne(e => e.Rental)
            .HasForeignKey(r => r.EventId); 
        //configure driver
        modelBuilder.Entity<Drivers>()
            .HasKey(d => new { d.UserId, d.EventId });

        modelBuilder.Entity<Rental>()
            .HasMany(d => d.users)
            .WithOne(u => u.Car)
            .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<Rental>()
            .HasOne(d => d.Event)
            .WithOne(e => e.Rental)
            .HasForeignKey(d => d.EventId); 
    }
    
}
