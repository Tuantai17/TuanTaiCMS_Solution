using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities
{
    /// <summary>
    /// Thực thể thông báo dành cho quản trị viên.
    /// Tạo thông báo khi có đơn hàng mới, thanh toán thành công, email gửi thất bại, v.v.
    /// </summary>
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Id của User (Admin/Staff) nhận thông báo. Null = tất cả admin.
        /// </summary>
        public int? TargetUserId { get; set; }

        /// <summary>
        /// Id của Customer nhận thông báo.
        /// </summary>
        public int? TargetCustomerId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Message { get; set; }

        /// <summary>
        /// Loại thông báo: NewOrder, PaymentSuccess, DeliverySuccess, EmailFailed, PasswordResetRequest, LowStock
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string NotificationType { get; set; } = string.Empty;

        /// <summary>
        /// Loại tham chiếu: Order, Product, Customer, EmailLog
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceType { get; set; }

        /// <summary>
        /// Mã tham chiếu (ví dụ: OrderId, ProductId)
        /// </summary>
        public int? ReferenceId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; }
    }
}
