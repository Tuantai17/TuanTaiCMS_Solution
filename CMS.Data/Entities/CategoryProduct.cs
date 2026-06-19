using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CMS.Data.Entities
{
    public class CategoryProduct
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; } // Đường dẫn ảnh đại diện loại sản phẩm

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual CategoryProduct? Parent { get; set; }
        public virtual ICollection<CategoryProduct>? Children { get; set; }

        public virtual ICollection<Product>? Products { get; set; }

        [NotMapped]
        public int Depth { get; set; } // Độ sâu cấp danh mục (dùng cho hiển thị cây)
    }
}

