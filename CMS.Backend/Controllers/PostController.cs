/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 15/5/2026
Mô tả: Controller quản lý bài viết, gồm hiển thị danh sách, xem chi tiết, thêm, sửa và xóa bài viết.
*/

// Nhóm thư viện phục vụ truy vấn database, xử lý MVC, upload file và tạo dropdown danh mục.
using System.Collections.Generic;
using System.Linq;
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization; // Buổi 5: Namespace cần thiết để dùng [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // PostController xử lý các request bắt đầu bằng /Post.
    // Buổi 5: [Authorize(Roles = "Admin,Staff")] bắt buộc phải đăng nhập với quyền Admin/Staff mới được vào các action bên dưới.
    [Authorize(Roles = "Admin,Staff")]
    public class PostController : Controller
    {
        // DbContext dùng để truy vấn bảng Posts và Categories.
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10; // Số bài viết mỗi trang trong admin

        // Constructor nhận context từ hệ thống Dependency Injection.
        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiển thị danh sách bài viết có phân trang.
        // Tham số id là mã danh mục, có thể null nếu người dùng không lọc danh mục.
        // page là trang hiện tại để phân trang.
        // Include lấy kèm Category để hiển thị tên danh mục ngoài View.
        // OrderByDescending đưa bài viết mới nhất lên đầu danh sách.
        public IActionResult Index(int? id, int page = 1)
        {
            var query = _context.Posts
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .AsQueryable();

            // Nếu id có giá trị, chỉ giữ lại bài viết thuộc danh mục đó.
            // Where lọc danh sách theo CategoryId trùng với id được truyền vào URL.
            if (id != null)
            {
                query = query.Where(p => p.CategoryId == id);
            }

            // Tính tổng số bài viết và phân trang
            var totalPosts = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalPosts / PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var posts = query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalPosts = totalPosts;

            return View(posts);
        }

        // Action Details hiển thị chi tiết một bài viết theo id.
        // FirstOrDefault trả về bài viết đầu tiên khớp điều kiện hoặc null nếu không có.
        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            // Nếu không tìm thấy bài viết thì trả về trang lỗi 404.
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // Action GET Create mở form thêm bài viết.
        // ViewBag.CategoryList chứa danh sách danh mục để hiển thị dropdown trong View.
        [HttpGet]
        public IActionResult Create()
        {
            LoadCategoryList();
            return View();
        }

        // Action POST Create nhận dữ liệu bài viết từ form và xử lý upload ảnh nếu có.
        // Ảnh được lưu trong wwwroot/uploads, database chỉ lưu đường dẫn tương đối.
        [HttpPost]
        public IActionResult Create(Post model, IFormFile? uploadImage)
        {
            // Bỏ qua validation của navigation property vì Category không được gửi từ form,
            // chỉ cần CategoryId là đủ để EF Core tạo liên kết.
            ModelState.Remove(nameof(Post.Category));

            if (!ModelState.IsValid)
            {
                LoadCategoryList(model.CategoryId);
                return View(model);
            }

            model.CreatedDate = model.CreatedDate == default ? DateTime.Now : model.CreatedDate;
            model.ImageUrl = SaveUploadImage(uploadImage) ?? model.ImageUrl;

            _context.Posts.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Bài viết đã được lưu và đăng thành công!";
            return RedirectToAction("Index");
        }

        // Action GET Edit dùng để mở form chỉnh sửa bài viết.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var post = _context.Posts.Find(id);

            if (post == null)
            {
                return NotFound();
            }

            LoadCategoryList(post.CategoryId);
            return View(post);
        }

        // Action POST Edit cập nhật bài viết.
        // Nếu không upload ảnh mới thì giữ nguyên ImageUrl cũ.
        [HttpPost]
        public IActionResult Edit(Post model, IFormFile? uploadImage)
        {
            var existingPost = _context.Posts.AsNoTracking().FirstOrDefault(p => p.Id == model.Id);

            if (existingPost == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Post.Category));

            if (!ModelState.IsValid)
            {
                LoadCategoryList(model.CategoryId);
                model.ImageUrl = existingPost.ImageUrl;
                return View(model);
            }

            model.ImageUrl = SaveUploadImage(uploadImage) ?? existingPost.ImageUrl;
            model.CreatedDate = model.CreatedDate == default ? existingPost.CreatedDate : model.CreatedDate;

            _context.Posts.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Bài viết đã được cập nhật thành công!";
            return RedirectToAction("Index");
        }

        // Action Delete xóa bài viết theo id nhận được từ giao diện.
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.Find(id);

            if (post != null)
            {
                _context.Posts.Remove(post);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Bài viết đã được xóa thành công!";
            }

            return RedirectToAction("Index");
        }

        // Action POST DeleteSelected nhận danh sách các id bài viết cần xóa.
        // ValidateAntiForgeryToken để chống tấn công giả mạo request giả mạo chéo trang (CSRF).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSelected(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một bài viết.";
                return RedirectToAction("Index");
            }

            int deletedCount = 0;
            foreach (var id in ids)
            {
                var post = _context.Posts.Find(id);
                if (post != null)
                {
                    _context.Posts.Remove(post);
                    deletedCount++;
                }
            }

            if (deletedCount > 0)
            {
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa thành công các bài viết đã chọn.";
            }

            return RedirectToAction("Index");
        }

        // Action POST ToggleFeatured bật/tắt trạng thái hiển thị trên trang chủ (AJAX).
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult ToggleFeatured(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài viết." });
            }

            post.IsFeatured = !post.IsFeatured;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                isFeatured = post.IsFeatured,
                message = post.IsFeatured
                    ? "Bài viết đã được bật hiển thị trên trang chủ."
                    : "Bài viết đã tắt hiển thị trên trang chủ."
            });
        }

        // API Endpoint hỗ trợ upload ảnh của CKEditor
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult UploadImage(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
            {
                return Json(new { uploaded = false, error = new { message = "Không nhận được file ảnh." } });
            }

            try
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(upload.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    upload.CopyTo(stream);
                }

                var url = "/uploads/" + fileName;
                return Json(new { uploaded = true, url = url });
            }
            catch (Exception ex)
            {
                return Json(new { uploaded = false, error = new { message = "Lỗi hệ thống: " + ex.Message } });
            }
        }

        // Hàm dùng chung để nạp dropdown danh mục cho form Create/Edit.
        private void LoadCategoryList(int? selectedCategoryId = null)
        {
            ViewBag.CategoryList = new SelectList(
                _context.Categories.ToList(),
                "Id",
                "Name",
                selectedCategoryId
            );
        }

        // Hàm lưu ảnh upload vào wwwroot/uploads và trả về đường dẫn tương đối.
        // Nếu người dùng không chọn file thì trả về null để controller giữ dữ liệu cũ.
        private string? SaveUploadImage(IFormFile? uploadImage)
        {
            if (uploadImage == null || uploadImage.Length == 0)
            {
                return null;
            }

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var fileName = Guid.NewGuid() + Path.GetExtension(uploadImage.FileName);
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                uploadImage.CopyTo(stream);
            }

            return "/uploads/" + fileName;
        }
    }
}
