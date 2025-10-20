using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Models;
using BCrypt.Net;

namespace CampusEvents.Services;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;

    public DatabaseSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Ensure database is created
        await _context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (await _context.Users.AnyAsync())
        {
            return; // Database already seeded
        }

        // Create sample users
        await SeedUsers();
        
        // Create sample organizations (already seeded in AppDbContext)
        // Create sample events
        await SeedEvents();
        
        // Create sample tickets
        await SeedTickets();
        
        // Create sample notifications
        await SeedNotifications();
        
        // Create sample organization memberships
        await SeedOrganizationMemberships();

        await _context.SaveChangesAsync();
    }

    private async Task SeedUsers()
    {
        var users = new List<User>
        {
            new User
            {
                Id = 2,
                Email = "john.student@campus.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                Name = "John Student",
                Role = UserRole.Student,
                ApprovalStatus = ApprovalStatus.Approved,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new User
            {
                Id = 3,
                Email = "jane.organizer@campus.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("organizer123"),
                Name = "Jane Organizer",
                Role = UserRole.Organizer,
                ApprovalStatus = ApprovalStatus.Approved,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new User
            {
                Id = 4,
                Email = "bob.organizer@campus.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("organizer123"),
                Name = "Bob Smith",
                Role = UserRole.Organizer,
                ApprovalStatus = ApprovalStatus.Approved,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new User
            {
                Id = 5,
                Email = "alice.student@campus.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                Name = "Alice Johnson",
                Role = UserRole.Student,
                ApprovalStatus = ApprovalStatus.Approved,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        _context.Users.AddRange(users);
    }

    private async Task SeedEvents()
    {
        var events = new List<Event>
        {
            new Event
            {
                Id = 1,
                Title = "Welcome Back Social",
                Description = "Join us for a fun evening of games, food, and networking to kick off the new semester!",
                EventDate = DateTime.UtcNow.AddDays(7),
                Location = "Student Union Building",
                Capacity = 200,
                TicketsIssued = 45,
                TicketType = TicketType.Free,
                Price = 0,
                Category = "Social",
                CategoryId = 2,
                OrganizerId = 3,
                OrganizationId = 1,
                IsApproved = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10),
                ImageUrl = "/images/welcome-back.jpg"
            },
            new Event
            {
                Id = 2,
                Title = "CS Career Fair",
                Description = "Meet with top tech companies and learn about internship and job opportunities in computer science.",
                EventDate = DateTime.UtcNow.AddDays(14),
                Location = "Engineering Building",
                Capacity = 150,
                TicketsIssued = 89,
                TicketType = TicketType.Free,
                Price = 0,
                Category = "Career",
                CategoryId = 5,
                OrganizerId = 3,
                OrganizationId = 2,
                IsApproved = true,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-8),
                ImageUrl = "/images/career-fair.jpg"
            },
            new Event
            {
                Id = 3,
                Title = "Basketball Championship",
                Description = "Watch our campus basketball team compete for the championship title!",
                EventDate = DateTime.UtcNow.AddDays(21),
                Location = "Sports Complex",
                Capacity = 500,
                TicketsIssued = 234,
                TicketType = TicketType.Paid,
                Price = 15.00m,
                Category = "Sports",
                CategoryId = 3,
                OrganizerId = 4,
                OrganizationId = 3,
                IsApproved = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                ImageUrl = "/images/basketball.jpg"
            },
            new Event
            {
                Id = 4,
                Title = "Art Exhibition Opening",
                Description = "Experience the creativity of our student artists in this stunning exhibition.",
                EventDate = DateTime.UtcNow.AddDays(28),
                Location = "Art Gallery",
                Capacity = 80,
                TicketsIssued = 12,
                TicketType = TicketType.Free,
                Price = 0,
                Category = "Arts",
                CategoryId = 4,
                OrganizerId = 3,
                OrganizationId = 4,
                IsApproved = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                ImageUrl = "/images/art-exhibition.jpg"
            },
            new Event
            {
                Id = 5,
                Title = "Python Workshop",
                Description = "Learn Python programming fundamentals in this hands-on workshop for beginners.",
                EventDate = DateTime.UtcNow.AddDays(35),
                Location = "Computer Lab 101",
                Capacity = 30,
                TicketsIssued = 8,
                TicketType = TicketType.Free,
                Price = 0,
                Category = "Workshops",
                CategoryId = 7,
                OrganizerId = 4,
                OrganizationId = 2,
                IsApproved = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                ImageUrl = "/images/python-workshop.jpg"
            }
        };

        _context.Events.AddRange(events);

        // Create analytics for each event
        var analytics = events.Select(e => new EventAnalytics
        {
            EventId = e.Id,
            ViewCount = Random.Shared.Next(50, 200),
            SaveCount = Random.Shared.Next(10, 50),
            TicketsSold = e.TicketsIssued,
            Revenue = e.TicketType == TicketType.Paid ? e.TicketsIssued * e.Price : 0,
            AttendanceRate = Random.Shared.Next(60, 95),
            LastUpdated = DateTime.UtcNow
        }).ToList();

        _context.EventAnalytics.AddRange(analytics);
    }

    private async Task SeedTickets()
    {
        var tickets = new List<Ticket>
        {
            new Ticket
            {
                Id = 1,
                EventId = 1,
                UserId = 2,
                UniqueCode = "TKT-001-ABC123",
                ClaimedAt = DateTime.UtcNow.AddDays(-5),
                IsRedeemed = false,
                PaymentStatus = PaymentStatus.Completed,
                PaymentAmount = 0
            },
            new Ticket
            {
                Id = 2,
                EventId = 1,
                UserId = 5,
                UniqueCode = "TKT-002-DEF456",
                ClaimedAt = DateTime.UtcNow.AddDays(-3),
                IsRedeemed = true,
                RedeemedAt = DateTime.UtcNow.AddDays(-1),
                PaymentStatus = PaymentStatus.Completed,
                PaymentAmount = 0
            },
            new Ticket
            {
                Id = 3,
                EventId = 2,
                UserId = 2,
                UniqueCode = "TKT-003-GHI789",
                ClaimedAt = DateTime.UtcNow.AddDays(-2),
                IsRedeemed = false,
                PaymentStatus = PaymentStatus.Completed,
                PaymentAmount = 0
            },
            new Ticket
            {
                Id = 4,
                EventId = 3,
                UserId = 5,
                UniqueCode = "TKT-004-JKL012",
                ClaimedAt = DateTime.UtcNow.AddDays(-1),
                IsRedeemed = false,
                PaymentStatus = PaymentStatus.Completed,
                PaymentAmount = 15.00m
            }
        };

        _context.Tickets.AddRange(tickets);
    }

    private async Task SeedNotifications()
    {
        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = 1,
                UserId = 2,
                EventId = 1,
                Type = NotificationType.EventReminder,
                Title = "Welcome Back Social - Tomorrow!",
                Message = "Don't forget about the Welcome Back Social happening tomorrow at 6 PM in the Student Union Building.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Notification
            {
                Id = 2,
                UserId = 5,
                EventId = 2,
                Type = NotificationType.EventUpdate,
                Title = "CS Career Fair - Updated Location",
                Message = "The CS Career Fair has been moved to the Engineering Building Room 201.",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Notification
            {
                Id = 3,
                UserId = 2,
                Type = NotificationType.SystemAlert,
                Title = "Welcome to Campus Events!",
                Message = "Welcome to our campus events platform! Start exploring events and save your favorites.",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        _context.Notifications.AddRange(notifications);
    }

    private async Task SeedOrganizationMemberships()
    {
        var memberships = new List<OrganizationMember>
        {
            new OrganizationMember
            {
                OrganizationId = 1,
                UserId = 3,
                Role = OrganizationRole.Manager,
                JoinedAt = DateTime.UtcNow.AddDays(-25)
            },
            new OrganizationMember
            {
                OrganizationId = 2,
                UserId = 3,
                Role = OrganizationRole.Member,
                JoinedAt = DateTime.UtcNow.AddDays(-20)
            },
            new OrganizationMember
            {
                OrganizationId = 3,
                UserId = 4,
                Role = OrganizationRole.Manager,
                JoinedAt = DateTime.UtcNow.AddDays(-20)
            },
            new OrganizationMember
            {
                OrganizationId = 4,
                UserId = 3,
                Role = OrganizationRole.Member,
                JoinedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        _context.OrganizationMembers.AddRange(memberships);
    }
}
