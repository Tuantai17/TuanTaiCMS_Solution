using System.ComponentModel.DataAnnotations;
/*
Sinh Vien: Nguyen Tuan Tai
Ma Sinh Vien: 2123110166
Lop: CCQ2311E
Ngay Tao: 15/5/2026
Mo ta: Dinh nghia thuc the san pham, luu tru thong tin ten, mo ta, gia, so luong ton kho va danh muc san pham.
*/

using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CMS.Data.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ten san pham khong duoc de trong")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }

        // Trang thai san pham moi (bat on se hien thi o section New tren trang chu)
        public bool IsNew { get; set; } = false;

        // Trang thai sale (bat on se hien thi gia khuyen mai)
        public bool IsSale { get; set; } = false;

        // Gia sale (chi co y nghia khi IsSale = true)
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; } = 0;

        // Trang thai san pham ban chay (bat on se uu tien hien thi o section Ban chay tren trang chu)
        public bool IsBestSelling { get; set; } = false;

        // Số thứ tự hiển thị riêng biệt cho từng trạng thái (0: mặc định theo ID mới nhất, >0: số nhỏ xếp trên)
        public int DisplayOrderNew { get; set; } = 0;
        public int DisplayOrderSale { get; set; } = 0;
        public int DisplayOrderBestSelling { get; set; } = 0;

        // Khoa ngoai noi toi CategoryProduct
        public int CategoryProductId { get; set; }

        [ForeignKey("CategoryProductId")]
        public virtual CategoryProduct? CategoryProduct { get; set; }

        // Bo suu tap anh chi tiet san pham
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}

