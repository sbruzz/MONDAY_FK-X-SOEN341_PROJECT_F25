namespace CampusEvents.Models
{
    public class Student : User
    {
        public required string MajorCode { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
