/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 15/5/2026
Mô tả: Controller quản lý bài viết, gồm hiển thị danh sách, xem chi tiết, thêm, sửa và xóa bài viết.
*/

// Nhóm thư viện phục vụ truy vấn database, xử lý MVC, upload file và tạo dropdown danh mục.
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization; // Buổi 5: Namespace cần thiết để dùng [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // PostController xử lý các request bắt đầu bằng /Post.
    // Buổi 5: [Authorize] bắt buộc phải đăng nhập mới được vào các action bên dưới.
    [Authorize]
    public class PostController : Controller
    {
        // DbContext dùng để truy vấn bảng Posts và Categories.
        private readonly ApplicationDbContext _context;

        // Constructor nhận context từ hệ thống Dependency Injection.
        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiển thị danh sách bài viết.
        // Tham số id là mã danh mục, có thể null nếu người dùng không lọc danh mục.
        // Include lấy kèm Category để hiển thị tên danh mục ngoài View.
        // OrderByDescending đưa bài viết mới nhất lên đầu danh sách.
        public IActionResult Index(int? id)
        {
            var posts = _context.Posts
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            // Nếu id có giá trị, chỉ giữ lại bài viết thuộc danh mục đó.
            // Where lọc danh sách theo CategoryId trùng với id được truyền vào URL.
            if (id != null)
            {
                posts = posts
                    .Where(p => p.CategoryId == id)
                    .ToList();
            }

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
            }

            return RedirectToAction("Index");
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
