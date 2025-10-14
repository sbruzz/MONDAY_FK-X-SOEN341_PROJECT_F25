using Microsoft.Data.Sqlite;

public class DbCSVCommunicator
{

    String DatabaseLocation = "../campusevents.db";

    public async Task extractToCSV(int EventId) {

        if (!File.Exists(DatabaseLocation))
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

            DataForCSV = DataForCSV.Remove(DataForCSV.Length - 1);

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

}