using System.ComponentModel.DataAnnotations;

namespace DotnetSupportTicketApi.DTOs
{
    public class UpdateTicketStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}