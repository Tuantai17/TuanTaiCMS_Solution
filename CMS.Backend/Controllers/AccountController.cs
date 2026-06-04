/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: Controller xử lý các tác vụ liên quan đến tài khoản: Đăng nhập, Đăng xuất và từ chối truy cập.
*/

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CMS.Data;

namespace CMS.Backend.Controllers
{
    // AccountController xử lý request bắt đầu bằng /Account.
    public class AccountController : Controller
    {
        // _context dùng để truy vấn bảng Users trong database.
        private readonly ApplicationDbContext _context;

        // Constructor nhận ApplicationDbContext từ Dependency Injection.
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action GET Login: Hiển thị trang đăng nhập.
        // Nếu người dùng đã đăng nhập rồi thì điều hướng thẳng vào Dashboard.
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // Action POST Login: Kiểm tra thông tin đăng nhập và cấp Cookie xác thực.
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Bước 1: Tìm người dùng trong Database theo Username và PasswordHash.
            // Lưu ý: Bài này dùng Plain Text để dễ học. Thực tế cần hash mật khẩu.
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

            if (user != null)
            {
                // Kiểm tra phân quyền: Chỉ cho phép tài khoản có Role là Admin truy cập hệ thống MVC quản trị.
                if (user.Role != "Admin")
                {
                    ViewBag.Error = "Tài khoản của bạn không có quyền truy cập hệ thống quản trị!";
                    return View();
                }

                // Bước 2: Thiết lập danh tính (Claims) - tập hợp thông tin nhận dạng người dùng.
                // Claim là các "mẩu thông tin" như tên, quyền hạn được đóng gói vào Cookie.
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role), // Lưu vai trò: Admin
                    new Claim("FullName", user.FullName)   // Claim tùy chỉnh lưu tên đầy đủ
                };

                // ClaimsIdentity là "chứng minh nhân dân" chứa các Claims trên.
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Bước 3: Đăng nhập chính thức - ghi Cookie vào trình duyệt của người dùng.
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            // Trường hợp sai tài khoản: thông báo lỗi qua ViewBag để hiển thị trên View.
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }

        // Action Logout: Xóa Cookie khỏi trình duyệt và điều hướng về trang Login.
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Action AccessDenied: Hiển thị trang thông báo từ chối truy cập (403).
        // Được gọi tự động khi người dùng không có quyền vào trang yêu cầu [Authorize(Roles=...)].
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
