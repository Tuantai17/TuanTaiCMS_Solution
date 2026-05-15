using System.ComponentModel.DataAnnotations;
/*
Sinh Viên: Nguy?n Tu?n Tài 
Mã Sinh Viên: 2123110166
L?p: CCQ2311E
Ngày T?o: 15/5/2026
Mô t?: ??nh ngh?a th?c th? danh m?c s?n ph?m, dùng ?? phân lo?i và qu?n lý các s?n ph?m trong h? th?ng.
*/

namespace CMS.Data.Entities
{
    public class CategoryProduct
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh m?c không ???c ?? tr?ng")]
        [StringLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        // Quan h?: M?t danh m?c có nhi?u s?n ph?m
        public virtual ICollection<Product>? Products { get; set; }
    }
}
