using Microsoft.EntityFrameworkCore;
using CampusEvents.Models;

namespace CampusEvents.Data;

/// <summary>
/// Entity Framework Core database context for the Campus Events application.
/// Manages database connections, entity sets, and relationship configurations.
/// </summary>
/// <remarks>
/// This class serves as the primary interface between the application and the database.
/// It provides access to all entity sets and configures relationships, constraints, and indexes.
/// 
/// Key Responsibilities:
/// - Entity set management (Users, Events, Tickets, etc.)
/// - Relationship configuration (one-to-many, many-to-many)
/// - Constraint definition (unique indexes, foreign keys)
/// - Delete behavior configuration (Cascade, Restrict, SetNull)
/// - Composite key configuration
/// 
/// Database Provider:
/// - Default: SQLite (development)
/// - Production: SQL Server or PostgreSQL (configurable)
/// 
/// Lifetime:
/// - Registered as Scoped service (one instance per HTTP request)
/// - Automatically disposed at end of request
/// - Thread-safe within a single request context
/// 
/// Usage Example:
/// ```csharp
/// public class MyService
/// {
///     private readonly AppDbContext _context;
///     
///     public MyService(AppDbContext context)
///     {
///         _context = context;
///     }
///     
///     public async Task<List<Event>> GetEventsAsync()
///     {
///         return await _context.Events
///             .Include(e => e.Organizer)
///             .ToListAsync();
///     }
/// }
/// ```
/// 
/// Important Notes:
/// - Always use async methods for database operations
/// - Use Include() for eager loading related entities
/// - Use AsNoTracking() for read-only queries
/// - Dispose context properly (handled automatically with DI)
/// </remarks>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the AppDbContext.
    /// </summary>
    /// <param name="options">Database context options configured in Program.cs.
    /// Contains connection string, database provider, and other configuration.</param>
    /// <remarks>
    /// The options are typically configured in Program.cs using:
    /// ```csharp
    /// builder.Services.AddDbContext<AppDbContext>(options =>
    ///     options.UseSqlite(connectionString));
    /// ```
    /// </remarks>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Core entity sets
    
    /// <summary>
    /// Database set for User entities
    /// </summary>
    public DbSet<User> Users { get; set; }
    
    /// <summary>
    /// Database set for Event entities
    /// </summary>
    public DbSet<Event> Events { get; set; }
    
    /// <summary>
    /// Database set for Ticket entities
    /// </summary>
    public DbSet<Ticket> Tickets { get; set; }
    
    /// <summary>
    /// Database set for Organization entities
    /// </summary>
    public DbSet<Organization> Organizations { get; set; }
    
    /// <summary>
    /// Database set for SavedEvent entities (many-to-many join table)
    /// </summary>
    public DbSet<SavedEvent> SavedEvents { get; set; }

    // Carpool system entities (US.04)
    
    /// <summary>
    /// Database set for Driver entities (carpool system)
    /// </summary>
    public DbSet<Driver> Drivers { get; set; }
    
    /// <summary>
    /// Database set for CarpoolOffer entities
    /// </summary>
    public DbSet<CarpoolOffer> CarpoolOffers { get; set; }
    
    /// <summary>
    /// Database set for CarpoolPassenger entities
    /// </summary>
    public DbSet<CarpoolPassenger> CarpoolPassengers { get; set; }

    // Room rental system entities (US.04)
    
    /// <summary>
    /// Database set for Room entities
    /// </summary>
    public DbSet<Room> Rooms { get; set; }
    
    /// <summary>
    /// Database set for RoomRental entities
    /// </summary>
    public DbSet<RoomRental> RoomRentals { get; set; }

    // Notification system entities

    /// <summary>
    /// Database set for Notification entities
    /// </summary>
    public DbSet<Notification> Notifications { get; set; }

    /// <summary>
    /// Configures the model relationships, constraints, and indexes.
    /// Called by Entity Framework Core when creating the database model.
    /// </summary>
    /// <param name="modelBuilder">Model builder instance used to configure entities.</param>
    /// <remarks>
    /// This method is called once when the DbContext is first created or when
    /// migrations are generated. It configures:
    /// 
    /// 1. **Composite Keys**: SavedEvent uses composite primary key (UserId, EventId)
    /// 2. **Foreign Keys**: All relationships with cascade/restrict behaviors
    /// 3. **Unique Constraints**: Email (User), UniqueCode (Ticket)
    /// 4. **Indexes**: Composite indexes for performance (RoomRental, Notification)
    /// 5. **Delete Behaviors**: 
    ///    - Cascade: Child entities deleted when parent deleted
    ///    - Restrict: Prevents deletion if child entities exist
    ///    - SetNull: Sets foreign key to null when parent deleted
    /// 
    /// Relationship Configuration:
    /// - One-to-Many: User → Events, Event → Tickets, etc.
    /// - Many-to-Many: User ↔ Event (via SavedEvent), CarpoolOffer ↔ User (via CarpoolPassenger)
    /// 
    /// Performance Considerations:
    /// - Indexes added for frequently queried columns
    /// - Composite indexes for multi-column queries
    /// - Foreign keys automatically indexed by EF Core
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call base implementation first
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

        // Configure Driver-User relationship (one-to-many: User can have multiple Drivers)
        // Organizers can add multiple drivers, students limited to one (enforced in business logic)
        modelBuilder.Entity<Driver>()
            .HasOne(d => d.User)
            .WithMany(u => u.Drivers)
            .HasForeignKey(d => d.UserId)
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

        // ===== Notification System Configuration =====

        // Configure Notification-User relationship
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for fetching unread notifications efficiently
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
    }

}
