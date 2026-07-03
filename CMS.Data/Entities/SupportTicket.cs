using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class SupportTicket
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // e.g., order, product, payment

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "new"; // new, in-progress, waiting-customer, resolved, closed

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "normal"; // low, normal, high, urgent

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [StringLength(100)]
        public string? RelatedOrderId { get; set; }

        [StringLength(100)]
        public string? RelatedOrderCode { get; set; }

        [StringLength(100)]
        public string? RelatedProductId { get; set; }

        public string? RelatedProductName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int UnreadCount { get; set; } = 0;
        public int CustomerUnreadCount { get; set; } = 0;

        public virtual ICollection<SupportTicketMessage> Messages { get; set; } = new List<SupportTicketMessage>();
    }
}
