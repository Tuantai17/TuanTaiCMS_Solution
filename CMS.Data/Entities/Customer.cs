using System.ComponentModel.DataAnnotations;
/*
Sinh Viên: Nguy?n Tu?n Tài 
Mã Sinh Viên: 2123110166
L?p: CCQ2311E
Ngày T?o: 15/5/2026
Mô t?: ??nh ngh?a th?c th? khách hàng, l?u thông tin cá nhân, tài kho?n và các ??n hàng liên quan.
*/

namespace CMS.Data.Entities
{
    // Khách hàng
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        [Required]
        public string Password { get; set; } // L?u m?t kh?u thô theo yêu c?u t?i gi?n

        public virtual ICollection<Order>? Orders { get; set; }
    }
}
