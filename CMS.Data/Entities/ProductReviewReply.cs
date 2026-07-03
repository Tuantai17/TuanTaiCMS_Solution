using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class ProductReviewReply
    {
        [Key]
        public int Id { get; set; }

        public int ProductReviewId { get; set; }

        public int AdminUserId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public bool IsOfficial { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(ProductReviewId))]
        public virtual ProductReview? ProductReview { get; set; }

        [ForeignKey(nameof(AdminUserId))]
        public virtual User? AdminUser { get; set; }
    }
}
