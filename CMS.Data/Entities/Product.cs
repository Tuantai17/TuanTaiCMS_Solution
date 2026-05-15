using System.ComponentModel.DataAnnotations;
/*
Sinh Viên: Nguy?n Tu?n Tài 
Mã Sinh Viên: 2123110166
L?p: CCQ2311E
Ngày T?o: 15/5/2026
Mô t?: ??nh ngh?a th?c th? s?n ph?m, l?u tr? thông tin tên, mô t?, giá, s? l??ng t?n kho và danh m?c s?n ph?m.
*/

using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên s?n ph?m không ???c ?? tr?ng")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }

        // Khóa ngo?i n?i t?i CategoryProduct
        public int CategoryProductId { get; set; }

        [ForeignKey("CategoryProductId")]
        public virtual CategoryProduct? CategoryProduct { get; set; }
    }
}
