using System.ComponentModel.DataAnnotations;
/*
Sinh Viên: Nguy?n Tu?n Tài 
Mã Sinh Viên: 2123110166
L?p: CCQ2311E
Ngày T?o: 15/5/2026
Mô t?: ??nh ngh?a th?c th? chi ti?t ??n hàng, l?u s?n ph?m, s? l??ng và ??n giá t?i th?i ?i?m mua.
*/

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
        public decimal UnitPrice { get; set; } // Giá t?i th?i ?i?m mua

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
