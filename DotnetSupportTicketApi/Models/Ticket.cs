namespace DotnetSupportTicketApi.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }
        public AppUser? CreatedByUser { get; set; }

        public int? AssignedToUserId { get; set; }
        public AppUser? AssignedToUser { get; set; }

        public List<Comment> Comments { get; set; } = new();
        public List<StatusHistory> StatusHistoryEntries { get; set; } = new();
    }
}
