using Microsoft.Data.Sqlite;
using CampusEvents.Models;
using CampusEvents.Data;

/// <summary>
/// Service for database communication and CSV data operations.
/// Handles database seeding, CSV import/export, and data initialization for development and testing.
/// </summary>
/// <remarks>
/// This service provides functionality for:
/// - Database seeding with test/demo data
/// - CSV export of event attendees
/// - Database initialization and setup
/// 
/// Key Features:
/// - Automatic admin account creation on first run
/// - Comprehensive demo data seeding (users, events, carpools, rooms)
/// - CSV export functionality for event attendee lists
/// - Idempotent seeding (safe to run multiple times)
/// 
/// Seeding Strategy:
/// - Checks for existing data before seeding (prevents duplicates)
/// - Creates admin account if it doesn't exist
/// - Seeds demo users (students, organizers, pending organizers)
/// - Seeds organizations and links them to organizers
/// - Seeds events with various categories and statuses
/// - Seeds drivers, carpool offers, rooms, and rentals
/// 
/// Security Notes:
/// - Default admin credentials are created for development only
/// - In production, admin account should be created manually
/// - Default passwords should be changed immediately after first login
/// 
/// CSV Export:
/// - Exports event attendee user IDs to CSV file
/// - File named as {EventId}.csv
/// - Used by organizers to export attendee lists
/// 
/// Lifetime:
/// - Registered as Transient service (new instance each time)
/// - Typically called once at application startup
/// </remarks>
public class DbCSVCommunicator
{
    /// <summary>
    /// Database context for accessing and modifying database entities.
    /// </summary>
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of DbCSVCommunicator.
    /// </summary>
    /// <param name="context">Database context for data operations.
    /// Used to access and modify database entities during seeding and CSV operations.</param>
    public DbCSVCommunicator(AppDbContext context)
    {
        _context = context;
    }

    // Use relative path that works on all platforms (Windows, Mac, Linux)
    /// <summary>
    /// Database connection string location.
    /// Static field storing the SQLite database connection string.
    /// </summary>
    /// <remarks>
    /// Format: "Data Source=campusevents.db"
    /// This is used for direct SQLite operations (CSV export) that bypass Entity Framework.
    /// </remarks>
    static String DatabaseLocation = $"Data Source=campusevents.db";
    
    /// <summary>
    /// Extracted database file path from connection string.
    /// Used for file operations on the database file.
    /// </summary>
    static String dbPath = DatabaseLocation.Replace("Data Source=", "").Trim();

    /// <summary>
    /// Initializes and seeds the database with test data.
    /// Creates admin account and demo users/events if they don't exist.
    /// </summary>
    /// <remarks>
    /// This method is called automatically on application startup to ensure
    /// the database has initial data for development and testing.
    /// 
    /// Seeding Process:
    /// 1. Creates admin account (if doesn't exist)
    /// 2. Seeds demo users (students, organizers, pending organizers)
    /// 3. Seeds organizations and links to organizers
    /// 4. Seeds events with various categories and approval statuses
    /// 5. Seeds drivers (pending approval)
    /// 6. Seeds carpool offers
    /// 7. Seeds rooms and room rentals
    /// 
    /// Idempotent Design:
    /// - Checks for existing data before creating
    /// - Safe to call multiple times
    /// - Won't create duplicate data
    /// 
    /// Default Credentials:
    /// - Admin: admin@campusevents.com / Admin@123
    /// - Demo Users: {email} / Demo@123
    /// 
    /// ⚠️ IMPORTANT: Change default passwords after first login!
    /// </remarks>
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


        // Seed comprehensive demo data
        if (!_context.Users.Any(u => u.Id >= 501 && u.Id <= 515))
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Demo@123");

            // Add students
            _context.Users.AddRange(
                new User { Id = 501, Email = "alice.smith@student.concordia.ca", PasswordHash = hashedPassword, Name = "Alice Smith", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, StudentId = "40101001", Program = "Computer Science", YearOfStudy = "3rd Year", PhoneNumber = "5141234567", CreatedAt = DateTime.UtcNow.AddDays(-30) },
                new User { Id = 502, Email = "bob.johnson@student.concordia.ca", PasswordHash = hashedPassword, Name = "Bob Johnson", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, StudentId = "40101002", Program = "Engineering", YearOfStudy = "2nd Year", PhoneNumber = "5141234568", CreatedAt = DateTime.UtcNow.AddDays(-25) },
                new User { Id = 503, Email = "carol.williams@student.concordia.ca", PasswordHash = hashedPassword, Name = "Carol Williams", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, StudentId = "40101003", Program = "Business", YearOfStudy = "4th Year", PhoneNumber = "5141234569", CreatedAt = DateTime.UtcNow.AddDays(-20) },
                new User { Id = 504, Email = "david.brown@student.concordia.ca", PasswordHash = hashedPassword, Name = "David Brown", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, StudentId = "40101004", Program = "Computer Science", YearOfStudy = "1st Year", PhoneNumber = "5141234570", CreatedAt = DateTime.UtcNow.AddDays(-15) },
                new User { Id = 505, Email = "eve.davis@student.concordia.ca", PasswordHash = hashedPassword, Name = "Eve Davis", Role = UserRole.Student, ApprovalStatus = ApprovalStatus.Approved, StudentId = "40101005", Program = "Arts", YearOfStudy = "2nd Year", PhoneNumber = "5141234571", CreatedAt = DateTime.UtcNow.AddDays(-10) }
            );

            // Add organizers
            _context.Users.AddRange(
                new User { Id = 511, Email = "tech.club@concordia.ca", PasswordHash = hashedPassword, Name = "Sarah Martinez", Role = UserRole.Organizer, ApprovalStatus = ApprovalStatus.Approved, Position = "President", Department = "Computer Science", PhoneNumber = "5149876543", CreatedAt = DateTime.UtcNow.AddDays(-60) },
                new User { Id = 512, Email = "music.society@concordia.ca", PasswordHash = hashedPassword, Name = "Mike Chen", Role = UserRole.Organizer, ApprovalStatus = ApprovalStatus.Approved, Position = "Coordinator", Department = "Fine Arts", PhoneNumber = "5149876544", CreatedAt = DateTime.UtcNow.AddDays(-55) },
                new User { Id = 513, Email = "sports.association@concordia.ca", PasswordHash = hashedPassword, Name = "Jennifer Lee", Role = UserRole.Organizer, ApprovalStatus = ApprovalStatus.Approved, Position = "Director", Department = "Athletics", PhoneNumber = "5149876545", CreatedAt = DateTime.UtcNow.AddDays(-50) }
            );

            // Add pending organizers (to show approval workflow)
            _context.Users.AddRange(
                new User { Id = 514, Email = "new.club@concordia.ca", PasswordHash = hashedPassword, Name = "Tom Wilson", Role = UserRole.Organizer, ApprovalStatus = ApprovalStatus.Pending, Position = "Founder", Department = "Business", PhoneNumber = "5149876546", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new User { Id = 515, Email = "pending.org@concordia.ca", PasswordHash = hashedPassword, Name = "Lisa Garcia", Role = UserRole.Organizer, ApprovalStatus = ApprovalStatus.Pending, Position = "President", Department = "Engineering", PhoneNumber = "5149876547", CreatedAt = DateTime.UtcNow.AddDays(-1) }
            );

            _context.SaveChanges();
            Console.WriteLine("✅ Demo users created (Students: 5, Organizers: 3, Pending: 2)");
            Console.WriteLine("   All passwords: Demo@123");
        }

        // Seed organizations
        if (!_context.Organizations.Any(o => o.Id >= 901 && o.Id <= 903))
        {
            _context.Organizations.AddRange(
                new Organization { Id = 901, Name = "Tech Innovators Club", Description = "Exploring cutting-edge technology and innovation", CreatedAt = DateTime.UtcNow.AddDays(-60) },
                new Organization { Id = 902, Name = "Concordia Music Society", Description = "Bringing music lovers together", CreatedAt = DateTime.UtcNow.AddDays(-55) },
                new Organization { Id = 903, Name = "Sports & Wellness Association", Description = "Promoting health and fitness on campus", CreatedAt = DateTime.UtcNow.AddDays(-50) }
            );
            _context.SaveChanges();

            // Link organizers to organizations
            var org511 = _context.Users.Find(511);
            var org512 = _context.Users.Find(512);
            var org513 = _context.Users.Find(513);
            if (org511 != null) org511.OrganizationId = 901;
            if (org512 != null) org512.OrganizationId = 902;
            if (org513 != null) org513.OrganizationId = 903;
            _context.SaveChanges();

            Console.WriteLine("✅ Organizations created and linked");
        }

        // Seed events
        if (!_context.Events.Any(e => e.Id >= 801 && e.Id <= 806))
        {
            // Get existing approved organizers to link events to
            var existingOrganizers = _context.Users
                .Where(u => u.Role == UserRole.Organizer && u.ApprovalStatus == ApprovalStatus.Approved)
                .Take(3)
                .ToList();

            // If no organizers exist, use demo organizers (511, 512, 513)
            // Otherwise use existing organizers
            int organizer1Id = existingOrganizers.Count > 0 ? existingOrganizers[0].Id : 511;
            int organizer2Id = existingOrganizers.Count > 1 ? existingOrganizers[1].Id : 512;
            int organizer3Id = existingOrganizers.Count > 2 ? existingOrganizers[2].Id : 513;

            // Get organization IDs (use null if no organizations exist)
            int? org1Id = existingOrganizers.Count > 0 ? existingOrganizers[0].OrganizationId : 901;
            int? org2Id = existingOrganizers.Count > 1 ? existingOrganizers[1].OrganizationId : 902;
            int? org3Id = existingOrganizers.Count > 2 ? existingOrganizers[2].OrganizationId : 903;

            _context.Events.AddRange(
                new Event { Id = 801, Title = "AI & Machine Learning Workshop", Description = "Learn the fundamentals of AI and ML", Category = EventCategory.Workshop, EventDate = DateTime.UtcNow.AddDays(7), Location = "H-110", Capacity = 50, TicketType = TicketType.Free, Price = 0, OrganizerId = organizer1Id, OrganizationId = org1Id, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = DateTime.UtcNow.AddDays(-20) },
                new Event { Id = 802, Title = "Open Mic Night", Description = "Showcase your musical talents", Category = EventCategory.Social, EventDate = DateTime.UtcNow.AddDays(10), Location = "D-110", Capacity = 100, TicketType = TicketType.Paid, Price = 5, OrganizerId = organizer2Id, OrganizationId = org2Id, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = DateTime.UtcNow.AddDays(-18) },
                new Event { Id = 803, Title = "Campus 5K Fun Run", Description = "Join us for a healthy morning run", Category = EventCategory.Sports, EventDate = DateTime.UtcNow.AddDays(14), Location = "Loyola Campus", Capacity = 200, TicketType = TicketType.Paid, Price = 10, OrganizerId = organizer3Id, OrganizationId = org3Id, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = DateTime.UtcNow.AddDays(-15) },
                new Event { Id = 804, Title = "Hackathon 2025", Description = "24-hour coding competition", Category = EventCategory.Competition, EventDate = DateTime.UtcNow.AddDays(21), Location = "H-Building", Capacity = 80, TicketType = TicketType.Free, Price = 0, OrganizerId = organizer1Id, OrganizationId = org1Id, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new Event { Id = 805, Title = "Career Fair 2025", Description = "Meet potential employers", Category = EventCategory.Career, EventDate = DateTime.UtcNow.AddDays(30), Location = "MB Building", Capacity = 500, TicketType = TicketType.Free, Price = 0, OrganizerId = organizer1Id, OrganizationId = org1Id, ApprovalStatus = ApprovalStatus.Pending, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new Event { Id = 806, Title = "Jazz Concert", Description = "Evening of smooth jazz", Category = EventCategory.Concert, EventDate = DateTime.UtcNow.AddDays(5), Location = "D-Theater", Capacity = 150, TicketType = TicketType.Paid, Price = 15, OrganizerId = organizer2Id, OrganizationId = org2Id, ApprovalStatus = ApprovalStatus.Approved, CreatedAt = DateTime.UtcNow.AddDays(-25) }
            );
            _context.SaveChanges();
            Console.WriteLine($"✅ Events created (6 events, 1 pending approval) - Linked to {existingOrganizers.Count} existing organizers");
        }

        // Seed drivers - all pending approval (Alice excluded for testing registration flow)
        if (!_context.Drivers.Any(d => d.Id >= 702 && d.Id <= 703))
        {
            _context.Drivers.AddRange(
                new Driver { Id = 702, UserId = 502, Capacity = 6, VehicleType = VehicleType.SUV, DriverType = DriverType.Student, LicensePlate = "XYZ789", Status = DriverStatus.Pending, SecurityFlags = "", History = "", CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new Driver { Id = 703, UserId = 503, Capacity = 3, VehicleType = VehicleType.Mini, DriverType = DriverType.Student, Status = DriverStatus.Pending, SecurityFlags = "", History = "", CreatedAt = DateTime.UtcNow.AddDays(-2) }
            );
            _context.SaveChanges();
            Console.WriteLine("✅ Drivers created (2 pending approval - Alice excluded for testing)");
        }

        // Seed carpool offers (only for Bob's driver, not Alice)
        if (!_context.CarpoolOffers.Any(co => co.Id == 602))
        {
            _context.CarpoolOffers.AddRange(
                new CarpoolOffer { Id = 602, EventId = 802, DriverId = 702, SeatsAvailable = 5, DepartureInfo = "Loyola Campus", DepartureTime = DateTime.UtcNow.AddDays(10).AddHours(-0.5), Status = CarpoolOfferStatus.Active, Latitude = 45.4586, Longitude = -73.6398, CreatedAt = DateTime.UtcNow.AddDays(-4) }
            );
            _context.SaveChanges();
            Console.WriteLine("✅ Carpool offers created (1 active ride)");
        }

        // Seed rooms
        if (!_context.Rooms.Any(r => r.Id >= 401 && r.Id <= 403))
        {
            // Get existing approved organizers to link rooms to
            var existingOrganizers = _context.Users
                .Where(u => u.Role == UserRole.Organizer && u.ApprovalStatus == ApprovalStatus.Approved)
                .Take(3)
                .ToList();

            // If no organizers exist, use demo organizers (511, 512, 513)
            int organizer1Id = existingOrganizers.Count > 0 ? existingOrganizers[0].Id : 511;
            int organizer2Id = existingOrganizers.Count > 1 ? existingOrganizers[1].Id : 512;
            int organizer3Id = existingOrganizers.Count > 2 ? existingOrganizers[2].Id : 513;

            _context.Rooms.AddRange(
                new Room { Id = 401, OrganizerId = organizer1Id, Name = "Tech Lab Main Room", Address = "H-920, Hall Building", Capacity = 30, Status = RoomStatus.Enabled, Amenities = "Projector,Whiteboard,WiFi", HourlyRate = 25, RoomInfo = "Perfect for workshops and meetings", AvailabilityStart = DateTime.UtcNow, AvailabilityEnd = DateTime.UtcNow.AddDays(90), CreatedAt = DateTime.UtcNow.AddDays(-40) },
                new Room { Id = 402, OrganizerId = organizer2Id, Name = "Music Practice Studio", Address = "D-205, D Building", Capacity = 15, Status = RoomStatus.Enabled, Amenities = "Piano,Sound System,Microphones", HourlyRate = 15, RoomInfo = "Soundproof practice space", AvailabilityStart = DateTime.UtcNow, AvailabilityEnd = DateTime.UtcNow.AddDays(60), CreatedAt = DateTime.UtcNow.AddDays(-35) },
                new Room { Id = 403, OrganizerId = organizer3Id, Name = "Fitness Training Room", Address = "PERFORM Centre", Capacity = 20, Status = RoomStatus.Enabled, Amenities = "Mats,Mirrors,Sound System", RoomInfo = "Great for yoga and fitness classes", AvailabilityStart = DateTime.UtcNow, AvailabilityEnd = DateTime.UtcNow.AddDays(120), CreatedAt = DateTime.UtcNow.AddDays(-30) }
            );
            _context.SaveChanges();
            Console.WriteLine($"✅ Rooms created (3 available rooms) - Linked to {existingOrganizers.Count} existing organizers");
        }

        // Seed room rentals
        if (!_context.RoomRentals.Any(rr => rr.Id >= 301 && rr.Id <= 305))
        {
            _context.RoomRentals.AddRange(
                new RoomRental { Id = 301, RoomId = 401, RenterId = 504, StartTime = DateTime.UtcNow.AddDays(3).AddHours(14), EndTime = DateTime.UtcNow.AddDays(3).AddHours(17), Purpose = "Study group session", ExpectedAttendees = 15, Status = RentalStatus.Approved, TotalCost = 75, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new RoomRental { Id = 302, RoomId = 402, RenterId = 505, StartTime = DateTime.UtcNow.AddDays(5).AddHours(18), EndTime = DateTime.UtcNow.AddDays(5).AddHours(20), Purpose = "Band practice", ExpectedAttendees = 8, Status = RentalStatus.Pending, TotalCost = 30, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new RoomRental { Id = 303, RoomId = 403, RenterId = 501, StartTime = DateTime.UtcNow.AddDays(6).AddHours(10), EndTime = DateTime.UtcNow.AddDays(6).AddHours(12), Purpose = "Yoga class", ExpectedAttendees = 12, Status = RentalStatus.Approved, CreatedAt = DateTime.UtcNow.AddDays(-4) },
                new RoomRental { Id = 304, RoomId = 401, RenterId = 502, StartTime = DateTime.UtcNow.AddDays(8).AddHours(13), EndTime = DateTime.UtcNow.AddDays(8).AddHours(16), Purpose = "Project presentation prep", ExpectedAttendees = 20, Status = RentalStatus.Pending, TotalCost = 75, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new RoomRental { Id = 305, RoomId = 402, RenterId = 503, StartTime = DateTime.UtcNow.AddDays(2).AddHours(15), EndTime = DateTime.UtcNow.AddDays(2).AddHours(17), Purpose = "Music recording session", ExpectedAttendees = 5, Status = RentalStatus.Approved, TotalCost = 30, CreatedAt = DateTime.UtcNow.AddDays(-6) }
            );
            _context.SaveChanges();
            Console.WriteLine("✅ Room rentals created (3 approved, 2 pending)");
        }

        Console.WriteLine("✅ Demo data seeding completed successfully!");
        Console.WriteLine("   📧 Login Credentials: any seeded email + password 'Demo@123'");
        Console.WriteLine("   🔑 Admin: admin@campusevents.com / Admin@123");
        Console.WriteLine("DbMainTest Finished");
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

        string CSVFilePath = EventId + ".csv";

        using (StreamWriter writer = new StreamWriter(CSVFilePath))
        {
            writer.Write(DataForCSV);
        }

    }
    

}