namespace DotnetSupportTicketApi.DTOs
{
    public class CommentResponse
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int AuthorUserId { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
