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
    public DbSet<Category> Categories { get; set; }
    public DbSet<EventAnalytics> EventAnalytics { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<OrganizationMember> OrganizationMembers { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<QrScanLog> QrScanLogs { get; set; }

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

        // Configure OrganizationMember as a many-to-many join table
        modelBuilder.Entity<OrganizationMember>()
            .HasKey(om => new { om.OrganizationId, om.UserId });

        modelBuilder.Entity<OrganizationMember>()
            .HasOne(om => om.Organization)
            .WithMany(o => o.Members)
            .HasForeignKey(om => om.OrganizationId);

        modelBuilder.Entity<OrganizationMember>()
            .HasOne(om => om.User)
            .WithMany(u => u.OrganizationMemberships)
            .HasForeignKey(om => om.UserId);

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

        // Configure Event-Category relationship
        modelBuilder.Entity<Event>()
            .HasOne(e => e.CategoryEntity)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Event-Analytics relationship (1:1)
        modelBuilder.Entity<EventAnalytics>()
            .HasOne(ea => ea.Event)
            .WithOne(e => e.Analytics)
            .HasForeignKey<EventAnalytics>(ea => ea.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure User-Notification relationship
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Event-Notification relationship
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Event)
            .WithMany(e => e.Notifications)
            .HasForeignKey(n => n.EventId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure User-AuditLog relationship
        modelBuilder.Entity<AuditLog>()
            .HasOne(al => al.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Ticket-QrScanLog relationship
        modelBuilder.Entity<QrScanLog>()
            .HasOne(qsl => qsl.Ticket)
            .WithMany(t => t.QrScanLogs)
            .HasForeignKey(qsl => qsl.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure User-QrScanLog relationship (Scanner)
        modelBuilder.Entity<QrScanLog>()
            .HasOne(qsl => qsl.Scanner)
            .WithMany(u => u.QrScanLogs)
            .HasForeignKey(qsl => qsl.ScannedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure unique email for User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Configure unique ticket code
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.UniqueCode)
            .IsUnique();

        // Configure indexes for performance
        modelBuilder.Entity<Event>()
            .HasIndex(e => e.EventDate);

        modelBuilder.Entity<Event>()
            .HasIndex(e => e.IsApproved);

        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead });

        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => new { al.UserId, al.Timestamp });

        modelBuilder.Entity<QrScanLog>()
            .HasIndex(qsl => qsl.TicketId);

        // Configure constraints
        modelBuilder.Entity<Event>()
            .ToTable(t => t.HasCheckConstraint("CK_Event_Price", "Price >= 0"));

        modelBuilder.Entity<Event>()
            .ToTable(t => t.HasCheckConstraint("CK_Event_Capacity", "Capacity > 0"));

        modelBuilder.Entity<Ticket>()
            .ToTable(t => t.HasCheckConstraint("CK_Ticket_PaymentAmount", "PaymentAmount >= 0"));

        // Seed initial data
        modelBuilder.Entity<Organization>().HasData(
            new Organization { Id = 1, Name = "Student Union", Description = "Official student union organization", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Organization { Id = 2, Name = "Computer Science Association", Description = "CS student group", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Organization { Id = 3, Name = "Athletics Department", Description = "Campus athletics and sports", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Organization { Id = 4, Name = "Arts Society", Description = "Student arts and culture organization", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Organization { Id = 5, Name = "Career Services", Description = "Professional development and career guidance", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Academic", Description = "Educational and academic events", IconName = "graduation-cap", IsActive = true },
            new Category { Id = 2, Name = "Social", Description = "Social gatherings and networking events", IconName = "users", IsActive = true },
            new Category { Id = 3, Name = "Sports", Description = "Athletic events and competitions", IconName = "trophy", IsActive = true },
            new Category { Id = 4, Name = "Arts", Description = "Cultural and artistic events", IconName = "palette", IsActive = true },
            new Category { Id = 5, Name = "Career", Description = "Professional development and career events", IconName = "briefcase", IsActive = true },
            new Category { Id = 6, Name = "Club Activities", Description = "Student club meetings and activities", IconName = "calendar", IsActive = true },
            new Category { Id = 7, Name = "Workshops", Description = "Educational workshops and training sessions", IconName = "tool", IsActive = true },
            new Category { Id = 8, Name = "Networking", Description = "Professional networking and meetup events", IconName = "network", IsActive = true }
        );

        // Create default admin user
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1, 
                Email = "admin@campus.edu", 
                PasswordHash = "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy", // admin123
                Name = "System Administrator", 
                Role = UserRole.Admin, 
                ApprovalStatus = ApprovalStatus.Approved,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
