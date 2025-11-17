using Microsoft.Data.Sqlite;
using CampusEvents.Models;
using CampusEvents.Data;


public class DbCSVCommunicator
{

    private readonly AppDbContext _context;

    public DbCSVCommunicator(AppDbContext context)
    {
        _context = context;
    }

    // Use relative path that works on all platforms (Windows, Mac, Linux)
    static String DatabaseLocation = $"Data Source=campusevents.db";
    static String dbPath = DatabaseLocation.Replace("Data Source=", "").Trim();

    public void Test()
    {
        Console.WriteLine("DbMainTest Launched");

        // Seed initial admin account if it doesn't exist
        if (!_context.Users.Any(u => u.Email == "admin@campusevents.com"))
        {
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            _context.Users.Add(new User
            {
                Email = "admin@campusevents.com",
                PasswordHash = adminPasswordHash,
                Name = "System Administrator",
                Role = UserRole.Admin,
                ApprovalStatus = ApprovalStatus.Approved,
                CreatedAt = DateTime.UtcNow
            });
            _context.SaveChanges();
            Console.WriteLine("✅ Initial admin account created:");
            Console.WriteLine("   Email: admin@campusevents.com");
            Console.WriteLine("   Password: Admin@123");
            Console.WriteLine("   ⚠️  IMPORTANT: Change this password after first login!");
        }

        // Only add test users if they don't already exist (prevents duplicate key error)
        if (!_context.Users.Any(u => u.Id >= 501 && u.Id <= 510))
        {
            _context.Users.AddRange(new User { Id = 501, Email = "user1@example.com", PasswordHash = "hash1", Name = "Alice Smith", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
                new User { Id = 502, Email = "user2@example.com", PasswordHash = "hash2", Name = "Bob Johnson", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = new DateTime(2025, 1, 2, 11, 0, 0, DateTimeKind.Utc) },
                new User { Id = 503, Email = "user3@example.com", PasswordHash = "hash3", Name = "Carol Williams", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = new DateTime(2025, 1, 3, 12, 0, 0, DateTimeKind.Utc) },
                new User { Id = 504, Email = "user4@example.com", PasswordHash = "hash4", Name = "David Brown", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = new DateTime(2025, 1, 4, 13, 0, 0, DateTimeKind.Utc) },
                new User { Id = 505, Email = "user5@example.com", PasswordHash = "hash5", Name = "Eve Davis", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = new DateTime(2025, 1, 5, 14, 0, 0, DateTimeKind.Utc) },
                new User { Id = 506, Email = "user6@example.com", PasswordHash = "hash6", Name = "Frank Miller", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = new DateTime(2025, 1, 6, 15, 0, 0, DateTimeKind.Utc) },
                new User { Id = 507, Email = "user7@example.com", PasswordHash = "hash7", Name = "Grace Wilson", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = new DateTime(2025, 1, 7, 16, 0, 0, DateTimeKind.Utc) },
                new User { Id = 508, Email = "user8@example.com", PasswordHash = "hash8", Name = "Hank Moore", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = new DateTime(2025, 1, 8, 17, 0, 0, DateTimeKind.Utc) },
                new User { Id = 509, Email = "user9@example.com", PasswordHash = "hash9", Name = "Ivy Taylor", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = new DateTime(2025, 1, 9, 18, 0, 0, DateTimeKind.Utc) },
                new User { Id = 510, Email = "user10@example.com", PasswordHash = "hash10", Name = "Jack Anderson", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = new DateTime(2025, 1, 10, 19, 0, 0, DateTimeKind.Utc) });
            _context.SaveChanges();
            Console.WriteLine("Test users added to database.");
        }
        else
        {
            Console.WriteLine("Test users already exist in database, skipping seed.");
        }

        // Seed test rental rooms
        SeedRentalRooms();
        
        // Seed test events
        SeedEvents();
        
        Console.WriteLine("DbMainTest Finished");
    }

    public void SeedRentalRooms()
    {
        try
        {
            // Check if rooms already exist
            var existingRooms = _context.Rooms.Count();
            if (existingRooms > 0)
            {
                Console.WriteLine($"✅ Rental rooms already exist ({existingRooms} found). Skipping seed.");
                return;
            }

            // Create sample rental rooms
            var rooms = new List<Room>
            {
                new Room
                {
                    Name = "Conference Room A",
                    Location = "Engineering Building, Floor 2, Room 201",
                    Description = "Large conference room with projector and whiteboard. Capacity: 30 people. Includes AV equipment.",
                    Capacity = 30,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    Name = "Study Room 1",
                    Location = "Library, Floor 1, Room 105",
                    Description = "Quiet study room perfect for group meetings. Capacity: 8 people. Includes whiteboard.",
                    Capacity = 8,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    Name = "Auditorium",
                    Location = "Main Building, Floor 1, Room 101",
                    Description = "Large auditorium with stage and sound system. Capacity: 200 people. Full AV setup available.",
                    Capacity = 200,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    Name = "Workshop Room",
                    Location = "Engineering Building, Floor 3, Room 305",
                    Description = "Hands-on workshop space with workbenches. Capacity: 20 people. Tools available upon request.",
                    Capacity = 20,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    Name = "Meeting Room B",
                    Location = "Business Building, Floor 2, Room 215",
                    Description = "Medium-sized meeting room with video conferencing. Capacity: 15 people.",
                    Capacity = 15,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    Name = "Computer Lab",
                    Location = "Computer Science Building, Floor 1, Room 110",
                    Description = "Computer lab with 25 workstations. Capacity: 25 people. Software available upon request.",
                    Capacity = 25,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    Name = "Seminar Room",
                    Location = "Science Building, Floor 2, Room 220",
                    Description = "Small seminar room with presentation equipment. Capacity: 12 people.",
                    Capacity = 12,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Rooms.AddRange(rooms);
            _context.SaveChanges();

            Console.WriteLine($"✅ Test rental rooms seeded successfully!");
            Console.WriteLine($"   - {rooms.Count} rooms created");
            Console.WriteLine($"   - Rooms: {string.Join(", ", rooms.Select(r => r.Name))}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error seeding rental rooms: {ex.Message}");
        }
    }

    public void SeedEvents()
    {
        try
        {
            // Check if we have enough events (seed if less than 8)
            var existingEvents = _context.Events.Count();
            if (existingEvents >= 8)
            {
                Console.WriteLine($"✅ Events already exist ({existingEvents} found). Skipping seed.");
                return;
            }
            
            if (existingEvents > 0)
            {
                Console.WriteLine($"⚠️  Found {existingEvents} existing events. Will add more to reach 8 total.");
            }

            // Get an approved organizer (or create one if none exists)
            var organizer = _context.Users
                .FirstOrDefault(u => u.Role == UserRole.Organizer && u.ApprovalStatus == ApprovalStatus.Approved);

            if (organizer == null)
            {
                // Create a test organizer if none exists
                var organizerPasswordHash = BCrypt.Net.BCrypt.HashPassword("Organizer@123");
                organizer = new User
                {
                    Email = "organizer@campusevents.com",
                    PasswordHash = organizerPasswordHash,
                    Name = "Test Organizer",
                    Role = UserRole.Organizer,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(organizer);
                _context.SaveChanges();
                Console.WriteLine("   Created test organizer account for events.");
            }

            // Create sample events with future dates
            var now = DateTime.UtcNow;
            var events = new List<Event>
            {
                new Event
                {
                    Title = "Tech Innovation Summit 2025",
                    Description = "Join us for a day of cutting-edge technology presentations, networking opportunities, and hands-on workshops. Featuring industry leaders and student projects.",
                    EventDate = now.AddDays(30),
                    Location = "Engineering Building, Main Auditorium, 123 University Ave",
                    Capacity = 200,
                    TicketType = TicketType.Free,
                    Price = 0,
                    Category = "Technology",
                    OrganizerId = organizer.Id,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt = now
                },
                new Event
                {
                    Title = "Spring Music Festival",
                    Description = "Celebrate spring with live music performances from student bands and local artists. Food trucks and activities throughout the day.",
                    EventDate = now.AddDays(45),
                    Location = "Campus Quad, Outdoor Stage",
                    Capacity = 500,
                    TicketType = TicketType.Free,
                    Price = 0,
                    Category = "Entertainment",
                    OrganizerId = organizer.Id,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt = now
                },
                new Event
                {
                    Title = "Career Fair 2025",
                    Description = "Connect with top employers and explore career opportunities. Bring your resume and dress professionally. Multiple companies from various industries.",
                    EventDate = now.AddDays(60),
                    Location = "Student Center, Hall A, 456 Campus Blvd",
                    Capacity = 300,
                    TicketType = TicketType.Free,
                    Price = 0,
                    Category = "Career",
                    OrganizerId = organizer.Id,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt = now
                },
                new Event
                {
                    Title = "Workshop: Web Development Bootcamp",
                    Description = "Intensive one-day workshop covering modern web development technologies. Hands-on coding sessions. Bring your laptop!",
                    EventDate = now.AddDays(20),
                    Location = "Computer Science Building, Room 110, 789 Tech St",
                    Capacity = 50,
                    TicketType = TicketType.Paid,
                    Price = 25.00m,
                    Category = "Education",
                    OrganizerId = organizer.Id,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt = now
                },
                new Event
                {
                    Title = "Charity Run for Education",
                    Description = "5K fun run to raise funds for student scholarships. All fitness levels welcome. Registration includes t-shirt and refreshments.",
                    EventDate = now.AddDays(25),
                    Location = "Campus Track, Sports Complex, 321 Athletic Way",
                    Capacity = 150,
                    TicketType = TicketType.Paid,
                    Price = 15.00m,
                    Category = "Sports",
                    OrganizerId = organizer.Id,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt = now
                },
                new Event
                {
                    Title = "Art Exhibition Opening",
                    Description = "View stunning artwork from student artists. Opening reception with refreshments. Exhibition runs for two weeks.",
                    EventDate = now.AddDays(15),
                    Location = "Arts Building, Gallery, 555 Creative Ave",
                    Capacity = 100,
                    TicketType = TicketType.Free,
                    Price = 0,
                    Category = "Arts",
                    OrganizerId = organizer.Id,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt = now
                },
                new Event
                {
                    Title = "Environmental Awareness Day",
                    Description = "Learn about sustainability, climate action, and environmental protection. Interactive exhibits, guest speakers, and eco-friendly activities.",
                    EventDate = now.AddDays(35),
                    Location = "Science Building, Atrium, 777 Green St",
                    Capacity = 120,
                    TicketType = TicketType.Free,
                    Price = 0,
                    Category = "Environment",
                    OrganizerId = organizer.Id,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt = now
                },
                new Event
                {
                    Title = "Gaming Tournament Finals",
                    Description = "Watch the finals of our campus gaming tournament. Multiple game categories. Prizes for winners. Spectators welcome!",
                    EventDate = now.AddDays(40),
                    Location = "Student Center, Game Room, 456 Campus Blvd",
                    Capacity = 80,
                    TicketType = TicketType.Free,
                    Price = 0,
                    Category = "Gaming",
                    OrganizerId = organizer.Id,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt = now
                }
            };

            _context.Events.AddRange(events);
            _context.SaveChanges();

            Console.WriteLine($"✅ Test events seeded successfully!");
            Console.WriteLine($"   - {events.Count} events created");
            Console.WriteLine($"   - Events: {string.Join(", ", events.Select(e => e.Title))}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error seeding events: {ex.Message}");
        }
    }

    public static void clearDatabase()
    {
        using var Connection = new SqliteConnection(DatabaseLocation);
        Connection.Open();
        using var command = Connection.CreateCommand();
        command.CommandText = "DELETE FROM Tickets";
        command.ExecuteNonQuery();
        command.CommandText = "DELETE FROM Events";
        command.ExecuteNonQuery();
        command.CommandText = "DELETE FROM Organizations";
        command.ExecuteNonQuery();
        command.CommandText = "DELETE FROM Users";
        command.ExecuteNonQuery();
        command.CommandText = "DELETE FROM SavedEvents";
        command.ExecuteNonQuery();
    }

    public static void extractToCSV(int EventId)
    {

        if (!File.Exists(dbPath))
        {
            Console.WriteLine("The path doesn't lead to the database, please rectify");
            return;
        }

        string DataForCSV = "";

        var SqlCommand = "SELECT UserId FROM Tickets WHERE EventId = @id";

        try
        {
            using var Connection = new SqliteConnection(DatabaseLocation);
            Connection.Open();

            using var Command = new SqliteCommand(SqlCommand, Connection);
            Command.Parameters.AddWithValue("@id", EventId);

            using var Reader = Command.ExecuteReader();
            if (Reader.HasRows)
            {
                while (Reader.Read())
                {

                    DataForCSV = DataForCSV + "," + Reader.GetInt64(0);
                }
            }
        }
        catch (SqliteException e)
        {
            Console.WriteLine(e.Message);
        }

        if (DataForCSV.Length > 0)
        {
            DataForCSV = DataForCSV.Remove(0, 1);
        }
        ;

        string CSVFilePath = EventId + ".csv";

        using (StreamWriter writer = new StreamWriter(CSVFilePath))
        {
            writer.Write(DataForCSV);
        }

    }
    

}