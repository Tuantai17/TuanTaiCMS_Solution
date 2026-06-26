using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities
{
    /// <summary>
    /// Thực thể nhật ký gửi email, ghi lại mọi lần gửi email từ hệ thống.
    /// </summary>
    public class EmailLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Loại email: OrderConfirmation, PaymentSuccess, DeliverySuccess, ForgotPassword
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string EmailType { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string RecipientEmail { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? RecipientName { get; set; }

        [Required]
        [MaxLength(500)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Loại tham chiếu: Order, Customer, ...
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceType { get; set; }

        /// <summary>
        /// Mã tham chiếu (ví dụ: OrderId, CustomerId)
        /// </summary>
        public int? ReferenceId { get; set; }

        /// <summary>
        /// Trạng thái: Pending, Sent, Failed
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? SentAt { get; set; }
    }
}
