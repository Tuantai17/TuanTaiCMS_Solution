using System.ComponentModel.DataAnnotations;
/*
Sinh Viên: Nguy?n Tu?n Tài 
Mã Sinh Viên: 2123110166
L?p: CCQ2311E
Ngày T?o: 15/5/2026
Mô t?: ??nh ngh?a th?c th? ??n hàng, l?u thông tin ngày ??t, khách hàng, tr?ng thái và chi ti?t ??n hàng.
*/

using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public int CustomerId { get; set; }

        public int Status { get; set; } // 0: Ch? duy?t, 1: ?ang giao, 2: ?ã xong

        public string? Notes { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
