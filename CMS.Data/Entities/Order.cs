using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public int CustomerId { get; set; }

        public int Status { get; set; }

        public string? Notes { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        public int PaymentStatus { get; set; } = 0;

        [MaxLength(100)]
        public string? TransactionCode { get; set; }

        public DateTime? PaymentDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        public DateTime? OrderConfirmationEmailSentAt { get; set; }
        public DateTime? PaymentSuccessEmailSentAt { get; set; }
        public DateTime? DeliverySuccessEmailSentAt { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
        public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    }
}
