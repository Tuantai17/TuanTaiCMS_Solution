using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    /// <summary>
    /// Thực thể lưu trữ token đặt lại mật khẩu cho khách hàng.
    /// Chỉ lưu bản hash của token, không lưu token nguyên bản.
    /// </summary>
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        /// <summary>
        /// Bản hash SHA256 của reset token (không lưu token gốc).
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime ExpiredAt { get; set; }

        public DateTime? UsedAt { get; set; }

        public bool IsUsed { get; set; } = false;

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
    }
}
