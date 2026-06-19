/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: API Controller quản lý thông tin bài viết tin tức, cung cấp dữ liệu JSON cho Frontend.
*/

using Microsoft.AspNetCore.Mvc; // Import thư viện hỗ trợ xây dựng API Controller
using Microsoft.EntityFrameworkCore; // Import thư viện Entity Framework Core hỗ trợ truy vấn cơ sở dữ liệu bất đồng bộ
using CMS.Data; // Import namespace chứa ApplicationDbContext
using CMS.Data.Entities; // Import namespace chứa các thực thể Entity

namespace CMS.Backend.Controllers
{
  [Route("api/[controller]")] // Định nghĩa đường dẫn gọi API. [controller] sẽ tự động lấy tên là "Posts"
  [ApiController] // Đánh dấu đây là API Controller để hệ thống tự động kiểm tra tính hợp lệ của dữ liệu đầu vào (Validation)
  public class PostsController : ControllerBase // Kế thừa ControllerBase để tối ưu hóa bộ nhớ cho API thuần JSON
  {
    private readonly ApplicationDbContext _context; // Khai báo trường đọc dữ liệu Context kết nối Database

    // Hàm khởi tạo (Constructor): "Tiêm" (Inject) ngữ cảnh dữ liệu ApplicationDbContext từ hệ thống vào Controller
    public PostsController(ApplicationDbContext context)
    {
      _context = context; // Gán context được tiêm vào cho biến nội bộ sử dụng
    }

    // 1. Chỉ định phương thức GET lấy toàn bộ danh sách bài viết thời trang
    // Đường dẫn truy cập: GET https://localhost:xxxx/api/posts
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      try
      {
        // Thực hiện truy vấn cơ sở dữ liệu bất đồng bộ
        var posts = await _context.Posts
          .Include(p => p.Category) // Tải kèm thông tin bảng Category liên kết
          .OrderByDescending(p => p.Id) // Sắp xếp bài viết theo thứ tự ID giảm dần (mới nhất lên đầu)
          .Select(p => new // Kỹ thuật gọt tỉa (Projection) - chỉ lấy các trường cần thiết phục vụ giao diện trang chủ
          {
            p.Id, // Mã định danh bài viết
            p.Title, // Tiêu đề bài viết
            p.ImageUrl, // Ảnh đại diện bài viết
            p.CreatedDate, // Ngày giờ tạo bài viết
            p.CategoryId, // Mã danh mục bài viết
            p.IsFeatured, // Trạng thái hiển thị trên trang chủ
            ShortDescription = string.IsNullOrWhiteSpace(p.Content)
              ? "Đang cập nhật nội dung tóm tắt cho bài viết..."
              : (p.Content.Length > 180 ? p.Content.Substring(0, 180) + "..." : p.Content),
            CategoryName = p.Category != null ? p.Category.Name : "Không xác định" // Kéo trực tiếp tên chuyên mục bài viết thay vì ID cộc lốc
          })
          .ToListAsync(); // Chuyển đổi kết quả bất đồng bộ thành kiểu danh sách List

        return Ok(posts); // Trả về kết quả JSON kèm theo mã trạng thái HTTP 200 OK
      }
      catch (Exception ex)
      {
        // Trả về mã lỗi 500 Internal Server Error nếu sập kết nối cơ sở dữ liệu hoặc lỗi xử lý hệ thống
        return StatusCode(500, new { message = "Lỗi xử lý hệ thống khi lấy danh sách bài viết", detail = ex.Message });
      }
    }

    // 2. Chỉ định phương thức GET lấy danh sách bài viết lọc theo mã ID chuyên mục
    // Đường dẫn truy cập: GET https://localhost:xxxx/api/posts/category/{categoryId}
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
      try
      {
        // Lọc danh sách các bài viết có CategoryId khớp với tham số truyền vào từ URL
        var posts = await _context.Posts
          .Include(p => p.Category)
          .Where(p => p.CategoryId == categoryId) // Thực hiện câu lệnh lọc Where trong cơ sở dữ liệu
          .OrderByDescending(p => p.Id) // Sắp xếp bài viết theo ID giảm dần (mới nhất lên đầu)
          .Select(p => new // Kỹ thuật gọt tỉa dữ liệu giúp giảm dung lượng gói tin truyền tải qua mạng
          {
            p.Id, // Mã định danh bài viết
            p.Title, // Tiêu đề bài viết
            p.ImageUrl, // Ảnh đại diện bài viết
            p.CreatedDate, // Ngày tạo bài viết
            p.CategoryId,
            p.IsFeatured,
            ShortDescription = string.IsNullOrWhiteSpace(p.Content)
              ? "Đang cập nhật nội dung tóm tắt cho bài viết..."
              : (p.Content.Length > 180 ? p.Content.Substring(0, 180) + "..." : p.Content),
            CategoryName = p.Category != null ? p.Category.Name : "Không xác định"
          })
          .ToListAsync(); // Chuyển kết quả bất đồng bộ sang dạng mảng

        return Ok(posts); // Trả về mảng JSON bài viết lọc theo chuyên mục và mã trạng thái HTTP 200 OK
      }
      catch (Exception ex)
      {
        // Bảo vệ hệ thống: Trả về mã lỗi 500 kèm thông tin báo lỗi chi tiết
        return StatusCode(500, new { message = "Lỗi hệ thống khi lọc bài viết theo chuyên mục", detail = ex.Message });
      }
    }

    // 3. API lấy danh sách bài viết nổi bật (IsFeatured = true) cho trang chủ
    // Đường dẫn truy cập: GET https://localhost:xxxx/api/posts/featured
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
      try
      {
        var posts = await _context.Posts
          .Include(p => p.Category)
          .Where(p => p.IsFeatured) // Chỉ lấy bài viết được đánh dấu hiển thị trang chủ
          .OrderByDescending(p => p.Id)
          .Select(p => new
          {
            p.Id,
            p.Title,
            p.ImageUrl,
            p.CreatedDate,
            p.IsFeatured,
            ShortDescription = string.IsNullOrWhiteSpace(p.Content)
              ? "Đang cập nhật nội dung tóm tắt cho bài viết..."
              : (p.Content.Length > 180 ? p.Content.Substring(0, 180) + "..." : p.Content),
            CategoryName = p.Category != null ? p.Category.Name : "Không xác định"
          })
          .ToListAsync();

        return Ok(posts);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Lỗi hệ thống khi lấy bài viết nổi bật", detail = ex.Message });
      }
    }

    // 4. Chỉ định phương thức GET lấy chi tiết 100% dữ liệu của duy nhất một bài viết theo ID
    // Đường dẫn truy cập: GET https://localhost:xxxx/api/posts/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(int id)
    {
      try
      {
        // Tìm bài viết đầu tiên khớp mã ID với tham số truyền vào bất đồng bộ
        var post = await _context.Posts
          .Include(p => p.Category) // Kèm theo bảng Category liên kết để lấy đầy đủ thông tin
          .FirstOrDefaultAsync(p => p.Id == id); // Thực hiện truy vấn lấy bản ghi đầu tiên hoặc giá trị mặc định null

        // Xử lý kịch bản lỗi bảo vệ hệ thống: ID truyền vào không tồn tại trong Database
        if (post == null)
        {
          // Trả về mã lỗi 404 Not Found kèm thông tin thông báo ngắn gọn bằng JSON
          return NotFound(new { message = "Không tìm thấy bài viết này trong hệ thống" });
        }

        // Trả về nguyên bản đối tượng thực thể bài viết (bao gồm cả trường Content chứa mã HTML) kèm mã 200 OK
        return Ok(post);
      }
      catch (Exception ex)
      {
        // Trả về mã lỗi 500 nếu xảy ra lỗi ngoại lệ ngầm định
        return StatusCode(500, new { message = "Lỗi xử lý hệ thống khi lấy chi tiết bài viết", detail = ex.Message });
      }
    }
  }
}
