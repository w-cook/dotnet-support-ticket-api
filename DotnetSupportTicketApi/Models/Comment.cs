namespace DotnetSupportTicketApi.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        public int AuthorUserId { get; set; }
        public AppUser? AuthorUser { get; set; }

        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
