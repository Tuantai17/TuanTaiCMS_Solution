/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 15/5/2026
Mô tả: Controller quản lý hiển thị danh sách người dùng trong hệ thống.
*/

// Nhóm thư viện dùng cho MVC và truy vấn database.
using CMS.Data;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Backend.Controllers
{
    // UserController xử lý request liên quan đến người dùng.
    public class UserController : Controller
    {
        // _context giúp truy cập bảng Users trong database.
        private readonly ApplicationDbContext _context;

        // Constructor nhận ApplicationDbContext từ Dependency Injection.
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index lấy toàn bộ danh sách người dùng.
        // ToList() thực thi truy vấn và trả dữ liệu về View/User/Index.cshtml.
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }
    }
}
