using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CMS.Data.Enums;

namespace CMS.Data.Entities
{
    public class OrderItemIssue
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int OrderDetailId { get; set; }

        public int ProductId { get; set; }

        public OrderItemIssueType IssueType { get; set; }

        public int OrderedQuantity { get; set; }

        public int FulfillableQuantity { get; set; }

        public int DamagedQuantity { get; set; }

        public int MissingQuantity { get; set; }

        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? InternalNote { get; set; }

        public OrderItemIssueStatus Status { get; set; }

        [MaxLength(255)]
        public string ReportedBy { get; set; } = string.Empty;

        public DateTime ReportedAt { get; set; }

        [MaxLength(255)]
        public string? CustomerDecision { get; set; }

        [MaxLength(2000)]
        public string? CustomerNote { get; set; }

        [MaxLength(255)]
        public string? ResolvedBy { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("OrderDetailId")]
        public virtual OrderDetail? OrderDetail { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
