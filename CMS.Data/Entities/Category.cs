/* 
/*
Sinh Viên: Nguyễn Tuấn Tài 
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 15/5/2026
Mô tả: Định nghĩa thực thể danh mục bài viết, dùng để phân loại và quản lý các bài viết trong hệ thống.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Data.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } // Tên danh mục (vd: Tin Giáo Dục)
        public string Description { get; set; }

        // Quan hệ: Một danh mục có nhiều bài viết
        public virtual ICollection<Post> Posts { get; set; }
    }
}

