/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 15/5/2026
Mô tả: Controller xử lý trang chủ, trang riêng tư và trang lỗi của website CMS.
*/

// Nhóm using: khai báo các thư viện và namespace cần dùng.
// CMS.Backend.Models chứa ErrorViewModel dùng cho trang lỗi.
// CMS.Data chứa ApplicationDbContext để truy vấn database.
// EntityFrameworkCore cung cấp Include để join bảng Category khi lấy bài viết.
using CMS.Backend.Models;
using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CMS.Backend.Controllers
{
    // HomeController quản lý các trang chung như trang chủ, privacy và error.
    public class HomeController : Controller
    {
        // _context dùng để truy vấn dữ liệu bài viết từ database.
        // _logger dùng để ghi log khi cần theo dõi hoạt động hoặc lỗi.
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        // Constructor nhận các dependency do ASP.NET Core tự động tiêm vào.
        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Action Index hiển thị trang Dashboard.
        // Include lấy kèm thông tin Category để View đọc được item.Category.Name.
        // OrderByDescending sắp xếp bài viết mới nhất theo CreatedDate lên đầu.
        // Không dùng Take(3) để Dashboard hiển thị đầy đủ dữ liệu bài viết theo yêu cầu.
        // ToList() thực thi truy vấn và chuyển kết quả thành danh sách.
        public IActionResult Index()
        {
            // Lấy đúng dữ liệu có sẵn trong database để hiển thị ở các ô thống kê Dashboard.
            ViewBag.TotalCategories = _context.Categories.Count();
            ViewBag.TotalPosts = _context.Posts.Count();
            ViewBag.TotalUsers = _context.Users.Count();

            var posts = _context.Posts
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            return View(posts);
        }

        // Action Privacy trả về trang thông tin riêng tư.
        public IActionResult Privacy()
        {
            return View();
        }

        // Cấu hình không cache trang lỗi để luôn hiển thị thông tin lỗi mới nhất.
        // ErrorViewModel chứa RequestId giúp tra cứu lỗi trong quá trình debug.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
