/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 15/5/2026
Mô tả: Controller quản lý danh mục bài viết, dùng để thêm, sửa, xóa và hiển thị danh mục trong hệ thống.
*/

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Buổi 5: Namespace cần thiết để dùng [Authorize]
using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using CMS.Data.Entities;

namespace CMS.Backend.Controllers
{
    // CategoryController nhận request liên quan đến đường dẫn /Category.
    // Kế thừa Controller để có thể trả về View, Redirect, NotFound...
    // Buổi 5: [Authorize] bắt buộc phải đăng nhập mới được truy cập các action bên dưới.
    [Authorize(Roles = "Admin,Staff")]
    public class CategoryController : Controller
    {
        // _context là biến dùng chung trong controller để thao tác với database.
        // readonly giúp đảm bảo biến chỉ được gán một lần trong constructor.
        private readonly ApplicationDbContext _context;

        // Constructor nhận ApplicationDbContext từ Dependency Injection.
        // ASP.NET Core tự tạo và truyền context vào khi controller được gọi.
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index dùng để hiển thị danh sách tất cả danh mục.
        // Include(c => c.Posts) để nạp trước danh sách bài viết nhằm hiển thị số lượng bài viết của từng danh mục.
        // ToList() thực thi truy vấn và lấy dữ liệu từ bảng Categories về bộ nhớ.
        // return View(data) truyền danh sách danh mục sang Views/Category/Index.cshtml.
        public IActionResult Index()
        {
            var data = _context.Categories.Include(c => c.Posts).ToList();
            return View(data);
        }

        // Action GET Create dùng để mở form thêm danh mục mới.
        // Hàm này chỉ trả về giao diện nhập liệu, chưa lưu dữ liệu vào database.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Action POST Create nhận dữ liệu người dùng gửi từ form thêm mới.
        // model chứa thông tin danh mục được bind từ các input asp-for trong View.
        // Add(model) đưa danh mục vào hàng chờ thêm mới của Entity Framework.
        // SaveChanges() ghi thay đổi thực sự xuống SQL Server.
        [HttpPost]
        public IActionResult Create(Category model)
        {
            _context.Categories.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // Action Delete nhận id danh mục cần xóa từ route.
        // Find(id) tìm danh mục theo khóa chính trong database.
        // Kiểm tra null để tránh lỗi khi id không tồn tại.
        // Kiểm tra bài viết liên quan trước khi xóa để tránh lỗi khóa ngoại và giữ toàn vẹn dữ liệu.
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);

            if (category != null)
            {
                bool hasPosts = _context.Posts.Any(p => p.CategoryId == id);
                if (hasPosts)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa danh mục \"{category.Name}\" đang có bài viết.";
                    return RedirectToAction("Index");
                }

                _context.Categories.Remove(category);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Danh mục đã được xóa thành công!";
            }

            return RedirectToAction("Index");
        }

        // Action POST DeleteSelected nhận danh sách các id danh mục cần xóa.
        // ValidateAntiForgeryToken để chống tấn công giả mạo request giả mạo chéo trang (CSRF).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSelected(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một danh mục.";
                return RedirectToAction("Index");
            }

            // Lấy danh sách tên các danh mục bị chặn xóa do chứa bài viết
            var restrictedCategories = _context.Categories
                .Where(c => ids.Contains(c.Id) && _context.Posts.Any(p => p.CategoryId == c.Id))
                .Select(c => c.Name)
                .ToList();

            if (restrictedCategories.Any())
            {
                var categoryNames = string.Join(", ", restrictedCategories.Select(name => $"\"{name}\""));
                TempData["ErrorMessage"] = $"Không thể xóa danh mục đang có bài viết: {categoryNames}.";
                return RedirectToAction("Index");
            }

            // Thực hiện xóa khi tất cả danh mục hợp lệ
            int deletedCount = 0;
            foreach (var id in ids)
            {
                var category = _context.Categories.Find(id);
                if (category != null)
                {
                    _context.Categories.Remove(category);
                    deletedCount++;
                }
            }

            if (deletedCount > 0)
            {
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa thành công các danh mục đã chọn.";
            }

            return RedirectToAction("Index");
        }

        // Action GET Edit dùng để mở form chỉnh sửa danh mục.
        // Nếu không tìm thấy dữ liệu theo id thì trả về lỗi 404 bằng NotFound().
        // Nếu tìm thấy thì truyền category sang View để đổ dữ liệu cũ lên form.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // Action POST Edit nhận dữ liệu sau khi người dùng bấm nút cập nhật.
        // Update(model) đánh dấu bản ghi cần sửa trong Entity Framework.
        // SaveChanges() lưu thay đổi xuống database.
        [HttpPost]
        public IActionResult Edit(Category model)
        {
            _context.Categories.Update(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
