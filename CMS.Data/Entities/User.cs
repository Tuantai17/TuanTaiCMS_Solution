/*
Sinh Viên: Nguyễn Tuấn Tài 
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 15/5/2026
Mô tả: Định nghĩa thực thể người dùng quản trị, phục vụ chức năng đăng nhập và phân quyền trong hệ thống.
*/

namespace CMS.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; } // Quản trị viên hoặc Biên tập viên
    }
}

