using System.ComponentModel.DataAnnotations;
using CMS.Data.Enums;

namespace CMS.Backend.Models
{
    public class ReportOrderItemIssueViewModel
    {
        public int OrderId { get; set; }

        public int OrderDetailId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public string? ProductImageUrl { get; set; }

        public int OrderedQuantity { get; set; }

        public int ReservedQuantity { get; set; }

        public int PhysicalQuantity { get; set; }

        public int FulfillableQuantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại sự cố.")]
        public OrderItemIssueType? IssueType { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng bị hư không hợp lệ.")]
        public int DamagedQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng bị thiếu không hợp lệ.")]
        public int MissingQuantity { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Lý do phải có ít nhất 10 ký tự và tối đa 1000 ký tự.")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Ghi chú nội bộ tối đa 2000 ký tự.")]
        public string? InternalNote { get; set; }
    }
}
