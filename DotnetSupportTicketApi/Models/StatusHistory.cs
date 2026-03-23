namespace DotnetSupportTicketApi.Models
{
    public class StatusHistory
    {
        public int Id { get; set; }

        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
