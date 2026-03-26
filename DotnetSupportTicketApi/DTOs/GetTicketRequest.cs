namespace DotnetSupportTicketApi.DTOs
{
    public class GetTicketsRequest
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
    }
}
