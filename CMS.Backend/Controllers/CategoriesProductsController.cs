/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: API Controller quản lý danh mục phân loại sản phẩm, cung cấp dữ liệu phân loại cho Frontend.
*/

using Microsoft.AspNetCore.Mvc; // Import thư viện hỗ trợ xây dựng các API Controller của ASP.NET Core
using Microsoft.EntityFrameworkCore; // Import thư viện Entity Framework Core hỗ trợ truy xuất Database bất đồng bộ
using CMS.Data; // Import namespace chứa lớp ngữ cảnh dữ liệu ApplicationDbContext
using CMS.Data.Entities; // Import namespace chứa các lớp thực thể Entity mẫu của Solution

namespace CMS.Backend.Controllers
{
  [Route("api/[controller]")] // Định nghĩa đường dẫn ánh xạ gọi API: api/CategoriesProducts
  [ApiController] // Kích hoạt thuộc tính xác thực dữ liệu đầu vào tự động (Validation)
  public class CategoriesProductsController : ControllerBase // Kế thừa ControllerBase để giải phóng bộ nhớ và tăng tốc phản hồi JSON
  {
    private readonly ApplicationDbContext _context; // Khai báo đối tượng trung gian kết nối cơ sở dữ liệu

    // Hàm khởi tạo (Constructor): Nhận đối tượng kết nối cơ sở dữ liệu thông qua cơ chế Dependency Injection (DI)
    public CategoriesProductsController(ApplicationDbContext context)
    {
      _context = context; // Gán đối tượng tiêm vào cho biến nội bộ
    }

    /// <summary>
    /// API lấy toàn bộ danh mục sản phẩm thời trang (Giao thức GET)
    /// Đường dẫn gọi dữ liệu: GET https://localhost:xxxx/api/CategoriesProducts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      try
      {
        // Thực hiện quét bảng cơ sở dữ liệu CategoriesProducts một cách bất đồng bộ
        var categories = await _context.CategoriesProducts
          .OrderBy(c => c.Id) // Thực hiện sắp xếp danh mục tăng dần theo mã ID định danh
          .Select(c => new // Áp dụng kỹ thuật gọt tỉa (Projection) để tối ưu băng thông truyền tải JSON
          {
            c.Id, // Mã ID của danh mục sản phẩm
            c.Name, // Tên danh mục (ví dụ: Điện thoại, Laptop, Phụ kiện...)
            c.Description, // Mô tả ngắn về danh mục sản phẩm tương ứng
            c.ParentId, // Mã ID của danh mục cha
            c.ImageUrl // Đường dẫn ảnh đại diện của danh mục sản phẩm
          })
          .ToListAsync(); // Chuyển đổi dữ liệu bất đồng bộ sang dạng danh sách mảng

        return Ok(categories); // Trả về mã thành công HTTP 200 OK đính kèm chuỗi JSON sạch
      }
      catch (Exception ex)
      {
        // Bảo vệ hệ thống: Nếu xảy ra lỗi kết nối SQL thì trả về mã trạng thái lỗi 500 kèm lời nhắn lý do
        return StatusCode(500, new
        {
          message = "Lỗi kết nối cơ sở dữ liệu hệ thống khi tải danh mục sản phẩm",
          detail = ex.Message
        });
      }
    }
  }
}
