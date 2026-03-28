using System.ComponentModel.DataAnnotations;

namespace DotnetSupportTicketApi.DTOs
{
    public class CreateCommentRequest
    {
        [Required]
        public int AuthorUserId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Body { get; set; } = string.Empty;
    }
}
