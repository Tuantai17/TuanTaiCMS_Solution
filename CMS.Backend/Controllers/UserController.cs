/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 15/5/2026
Mô tả: Controller quản lý thành viên, gồm hiển thị danh sách, thêm, sửa và xóa người dùng trong hệ thống.
*/

// Nhóm thư viện dùng cho MVC và truy vấn database.
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // Action GET Create dùng để mở form thêm thành viên mới.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Action POST Create nhận thông tin thành viên mới và kiểm tra trùng Username.
        [HttpPost]
        public IActionResult Create(User model)
        {
            var checkExist = _context.Users.Any(u => u.Username == model.Username);

            if (checkExist)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã có người dùng!");
                return View(model);
            }

            _context.Users.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // Action GET Edit hiển thị form kèm dữ liệu cũ của User.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Action POST Edit lưu thay đổi.
        // Nếu NewPassword rỗng thì giữ lại PasswordHash cũ để tránh mất mật khẩu.
        [HttpPost]
        public IActionResult Edit(User model, string? NewPassword)
        {
            var existingUser = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == model.Id);

            if (existingUser == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                model.PasswordHash = NewPassword;
            }
            else
            {
                model.PasswordHash = existingUser.PasswordHash;
            }

            _context.Users.Update(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // Action Delete xóa thành viên theo id nhận được từ giao diện.
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);

            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
