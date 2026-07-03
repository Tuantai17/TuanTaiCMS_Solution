using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class SupportTicketMessage
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TicketId { get; set; } = string.Empty;

        [ForeignKey("TicketId")]
        public virtual SupportTicket Ticket { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string SenderType { get; set; } = string.Empty; // customer, staff, system

        [Required]
        public string SenderName { get; set; } = string.Empty;

        public string? Content { get; set; }

        public string? Attachments { get; set; } // JSON serialized array of strings or object if multiple

        [StringLength(50)]
        public string? StickerCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
