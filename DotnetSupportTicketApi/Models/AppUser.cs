namespace DotnetSupportTicketApi.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public List<Ticket> CreatedTickets { get; set; } = new();
        public List<Ticket> AssignedTickets { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
    }
}
