using System.ComponentModel.DataAnnotations;

namespace DotnetSupportTicketApi.DTOs
{
    public class CreateTicketRequest
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = "Medium";

        [Required]
        public int CreatedByUserId { get; set; }

        public int? AssignedToUserId { get; set; }
    }
}
