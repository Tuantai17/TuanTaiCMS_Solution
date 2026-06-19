/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 18/6/2026
Mô tả: Định nghĩa thực thể banner quảng cáo, lưu trữ tiêu đề, đường dẫn hình ảnh, thứ tự và trạng thái ẩn hiện.
*/

using System;

namespace CMS.Data.Entities
{
    public class Banner
    {
        public int Id { get; set; }
        
        public string? Title { get; set; } // Tiêu đề hoặc mô tả banner (có thể null)
        
        public string ImageUrl { get; set; } // Đường dẫn ảnh (URL tuyệt đối hoặc đường dẫn tương đối upload)
        
        public int DisplayOrder { get; set; } = 0; // Thứ tự hiển thị
        
        public bool IsVisible { get; set; } = true; // Trạng thái hiển thị (ẩn/hiện)
        
        public string? TargetUrl { get; set; } // Đường dẫn đích khi click vào banner (ví dụ: /products hoặc URL ngoài)
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
