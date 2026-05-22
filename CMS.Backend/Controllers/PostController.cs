/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 15/5/2026
Mô tả: Controller quản lý bài viết, gồm hiển thị danh sách, xem chi tiết và thêm bài viết mới.
*/

// Nhóm thư viện phục vụ truy vấn database, xử lý MVC và tạo dropdown danh mục.
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // PostController xử lý các request bắt đầu bằng /Post.
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
        // ViewBag.CategoryId chứa danh sách danh mục để hiển thị dropdown trong View.
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(
                _context.Categories.ToList(),
                "Id",
                "Name"
            );

            return View();
        }

        // Action POST Create nhận dữ liệu bài viết từ form.
        // CreatedDate được gán bằng thời gian hiện tại để ghi nhận ngày đăng.
        // Add và SaveChanges lần lượt thêm vào bộ nhớ tạm và lưu xuống database.
        [HttpPost]
        public IActionResult Create(Post model)
        {
            model.CreatedDate = DateTime.Now;

            _context.Posts.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
