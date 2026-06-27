using System;
using System.ComponentModel.DataAnnotations;
using CMS.Data.Enums;

namespace CMS.Backend.Models
{
    public class ResolveOrderItemIssueViewModel
    {
        public int IssueId { get; set; }

        public int OrderId { get; set; }

        public int OrderDetailId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public string? ProductImageUrl { get; set; }

        public int OrderedQuantity { get; set; }

        public int FulfillableQuantity { get; set; }

        public int DamagedQuantity { get; set; }

        public int MissingQuantity { get; set; }

        public string IssueReason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn phương án xử lý.")]
        public CustomerIssueDecision Decision { get; set; }

        public int? AdjustedQuantity { get; set; }

        public DateTime? ExpectedRestockDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức liên hệ.")]
        public string ContactMethod { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Ghi chú tối đa 2000 ký tự.")]
        public string? CustomerNote { get; set; }

        public decimal OldTotal { get; set; }

        public decimal NewTotal { get; set; }

        public decimal DifferenceAmount { get; set; }

        public decimal RefundAmount { get; set; }
    }
}
