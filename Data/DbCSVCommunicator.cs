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

    static String DatabaseLocation = $"Data Source=C:/Users/Aeon/Documents/GitHub/MONDAY_FK-X-SOEN341_PROJECT_F25/campusevents.db";
    static String dbPath = DatabaseLocation.Replace("Data Source=", "").Trim();

    public void Test()
    {
        Console.WriteLine("DbMainTest Launched");
        /**
        Ticket[] tickets = GenerateTickets();

        for (int i = 0; i < tickets.Length; i++)
        {
            addTicketIntoDb(tickets[i]);
        }
        **/

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
        extractToCSV(533);
        Console.WriteLine("DbMainTest Finished");
    }

    private void addTicketIntoDb(Ticket ticket)
    {
        _context.Tickets.Add(ticket);
        _context.SaveChanges();
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
                    DataForCSV = DataForCSV + ",";
                }
            }

            if (DataForCSV.Length > 0)
            {
                DataForCSV = DataForCSV.Remove(DataForCSV.Length - 1);
            }
        }
        catch (SqliteException e)
        {
            Console.WriteLine(e.Message);
        }

        string CSVFilePath = EventId + ".csv";

        using (StreamWriter writer = new StreamWriter(CSVFilePath))
        {
            writer.Write(DataForCSV);
        }

    }


    public static Ticket[] GenerateTickets()
    {

        Ticket[] ticketList = new Ticket[10];

        for (int i = 1; i <= 10; i++)
        {
            var ticket = new Ticket
            {
                Id = i,
                EventId = i % 2 == 0 ? 32452 : 533, 
                UserId = i, 
                UniqueCode = Guid.NewGuid().ToString(), 
                QrCodeImage = null,
                ClaimedAt = DateTime.UtcNow,
                RedeemedAt = null,
                IsRedeemed = false,
            };
            ticketList[i-1] = ticket;
        }
        return ticketList;

    }
    

}