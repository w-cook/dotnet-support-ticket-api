namespace DotnetSupportTicketApi.DTOs
{
    public class StatusHistoryResponse
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}
