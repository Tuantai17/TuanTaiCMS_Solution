using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CMS.Data.Enums;

namespace CMS.Data.Entities
{
    public class ProductReview
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int OrderId { get; set; }

        public int OrderDetailId { get; set; }

        public int CustomerId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(150)]
        public string? Title { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

        public bool IsVerifiedPurchase { get; set; } = true;

        public bool IsEdited { get; set; }

        [MaxLength(500)]
        public string? ModerationReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product? Product { get; set; }

        [ForeignKey(nameof(OrderId))]
        public virtual Order? Order { get; set; }

        [ForeignKey(nameof(OrderDetailId))]
        public virtual OrderDetail? OrderDetail { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<ProductReviewImage> Images { get; set; } = new List<ProductReviewImage>();
        public virtual ICollection<ProductReviewReply> Replies { get; set; } = new List<ProductReviewReply>();
    }
}
