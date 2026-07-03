using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class ProductReviewImage
    {
        [Key]
        public int Id { get; set; }

        public int ProductReviewId { get; set; }

        [Required]
        [MaxLength(300)]
        public string ImageUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(ProductReviewId))]
        public virtual ProductReview? ProductReview { get; set; }
    }
}
