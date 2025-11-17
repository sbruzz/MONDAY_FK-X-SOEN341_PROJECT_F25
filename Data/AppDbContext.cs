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

    // Carpool system entities (US.04)
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<CarpoolOffer> CarpoolOffers { get; set; }
    public DbSet<CarpoolPassenger> CarpoolPassengers { get; set; }

    // Room rental system entities (US.04)
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomRental> RoomRentals { get; set; }
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

        // ===== Carpool System Configuration (US.04) =====

        // Configure Driver-User relationship (one-to-one)
        modelBuilder.Entity<Driver>()
            .HasOne(d => d.User)
            .WithOne(u => u.DriverProfile)
            .HasForeignKey<Driver>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure CarpoolOffer-Driver relationship
        modelBuilder.Entity<CarpoolOffer>()
            .HasOne(co => co.Driver)
            .WithMany(d => d.CarpoolOffers)
            .HasForeignKey(co => co.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure CarpoolOffer-Event relationship
        modelBuilder.Entity<CarpoolOffer>()
            .HasOne(co => co.Event)
            .WithMany(e => e.CarpoolOffers)
            .HasForeignKey(co => co.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure CarpoolPassenger-CarpoolOffer relationship
        modelBuilder.Entity<CarpoolPassenger>()
            .HasOne(cp => cp.Offer)
            .WithMany(co => co.Passengers)
            .HasForeignKey(cp => cp.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure CarpoolPassenger-User relationship
        modelBuilder.Entity<CarpoolPassenger>()
            .HasOne(cp => cp.Passenger)
            .WithMany(u => u.CarpoolPassengers)
            .HasForeignKey(cp => cp.PassengerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Room Rental System Configuration (US.04) =====

        // Configure Room-User relationship (organizer)
        modelBuilder.Entity<Room>()
            .HasOne(r => r.Organizer)
            .WithMany(u => u.ManagedRooms)
            .HasForeignKey(r => r.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure RoomRental-Room relationship
        modelBuilder.Entity<RoomRental>()
            .HasOne(rr => rr.Room)
            .WithMany(r => r.Rentals)
            .HasForeignKey(rr => rr.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure RoomRental-User relationship (renter)
        modelBuilder.Entity<RoomRental>()
            .HasOne(rr => rr.Renter)
            .WithMany(u => u.RoomRentals)
            .HasForeignKey(rr => rr.RenterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for preventing double booking (overlapping rentals)
        modelBuilder.Entity<RoomRental>()
            .HasIndex(rr => new { rr.RoomId, rr.StartTime, rr.EndTime });
    }
    
}
