using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Gia tai thoi diem mua

        // Cac thuoc tinh xu ly su co chuan bi hang
        public int OriginalQuantity { get; set; }
        public int FulfillableQuantity { get; set; }
        public int? AdjustedQuantity { get; set; }
        public int DamagedQuantity { get; set; }
        public int MissingQuantity { get; set; }
        public CMS.Data.Enums.OrderItemStatus ItemStatus { get; set; } = CMS.Data.Enums.OrderItemStatus.Normal;
        
        public string? IssueType { get; set; }
        public string? IssueReason { get; set; }
        public string? InternalNote { get; set; }
        public string? CustomerDecision { get; set; }
        public DateTime? IssueReportedAt { get; set; }
        public string? IssueReportedBy { get; set; }
        public DateTime? CustomerConfirmedAt { get; set; }
        public string? CustomerConfirmedBy { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public virtual ProductReview? ProductReview { get; set; }
    }
}
