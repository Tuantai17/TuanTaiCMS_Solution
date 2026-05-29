/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: API Controller quản lý sản phẩm thời trang và công nghệ, cung cấp dữ liệu JSON cho Frontend.
*/

using Microsoft.AspNetCore.Mvc; // Import thư viện hỗ trợ xây dựng các API Controller của ASP.NET Core
using Microsoft.EntityFrameworkCore; // Import thư viện Entity Framework Core hỗ trợ truy xuất Database bất đồng bộ
using CMS.Data; // Import namespace chứa lớp ngữ cảnh dữ liệu ApplicationDbContext
using CMS.Data.Entities; // Import namespace chứa các lớp thực thể Entity mẫu của Solution

namespace CMS.Backend.Controllers
{
  [Route("api/[controller]")] // Định nghĩa đường dẫn ánh xạ gọi API: api/Products
  [ApiController] // Kích hoạt thuộc tính xác thực dữ liệu đầu vào tự động (Validation)
  public class ProductsController : ControllerBase // Kế thừa ControllerBase để tối ưu bộ nhớ và tăng tốc độ xử lý gói tin JSON
  {
    private readonly ApplicationDbContext _context; // Khai báo đối tượng trung gian kết nối cơ sở dữ liệu

    // Hàm khởi tạo (Constructor): Nhận đối tượng kết nối cơ sở dữ liệu thông qua cơ chế Dependency Injection (DI)
    public ProductsController(ApplicationDbContext context)
    {
      _context = context; // Gán đối tượng tiêm vào cho biến nội bộ
    }

    // 1. Chỉ định phương thức GET lấy toàn bộ danh sách sản phẩm
    // Đường dẫn gọi dữ liệu: GET https://localhost:xxxx/api/products
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      try
      {
        // Thực hiện quét bảng cơ sở dữ liệu Products dưới SQL Server một cách bất đồng bộ
        var products = await _context.Products
          .OrderByDescending(p => p.Id) // Sắp xếp sản phẩm theo thứ tự ID giảm dần (mới nhất lên đầu)
          .Select(p => new // Kỹ thuật gọt tỉa (Projection) - chỉ lấy các trường cần thiết ra trang chủ để tối ưu băng thông
          {
            p.Id, // Mã ID sản phẩm
            p.Name, // Tên sản phẩm
            p.Price, // Đơn giá sản phẩm
            p.ImageUrl, // Ảnh đại diện sản phẩm
            p.StockQuantity, // Số lượng tồn kho sản phẩm
            p.CategoryProductId // Mã danh mục sản phẩm liên kết
          })
          .ToListAsync(); // Chuyển đổi kết quả bất đồng bộ sang dạng danh sách mảng

        return Ok(products); // Trả về kết quả mảng JSON và mã trạng thái HTTP 200 OK
      }
      catch (Exception ex)
      {
        // Trả về lỗi 500 Internal Server Error kèm thông báo lỗi chi tiết nếu có sự cố hệ thống
        return StatusCode(500, new { message = "Lỗi hệ thống khi tải danh sách sản phẩm", detail = ex.Message });
      }
    }

    // 2. Chỉ định phương thức GET lấy danh sách sản phẩm theo mã ID danh mục phân loại
    // Đường dẫn gọi dữ liệu: GET https://localhost:xxxx/api/products/categoryproduct/{categoryProductId}
    [HttpGet("categoryproduct/{categoryProductId}")]
    public async Task<IActionResult> GetByCategoryProduct(int categoryProductId)
    {
      try
      {
        // Thực hiện lọc danh sách các sản phẩm có CategoryProductId khớp với tham số truyền vào từ URL
        var products = await _context.Products
          .Where(p => p.CategoryProductId == categoryProductId) // Lọc dữ liệu trong DB
          .OrderByDescending(p => p.Id) // Sắp xếp sản phẩm mới nhất lên đầu
          .Select(p => new // Kỹ thuật gọt tỉa dữ liệu giúp giảm nhẹ gói tin truyền tải qua mạng
          {
            p.Id, // Mã ID sản phẩm
            p.Name, // Tên sản phẩm
            p.Price, // Đơn giá sản phẩm
            p.ImageUrl, // Ảnh sản phẩm
            p.StockQuantity, // Số lượng tồn kho
            p.CategoryProductId // Mã danh mục
          })
          .ToListAsync(); // Chuyển kết quả sang dạng danh sách mảng

        return Ok(products); // Trả về mảng JSON kết quả lọc sản phẩm và mã trạng thái HTTP 200 OK
      }
      catch (Exception ex)
      {
        // Trả về lỗi 500 nếu xảy ra sự cố ngoại lệ ngầm định
        return StatusCode(500, new { message = "Lỗi hệ thống khi lọc sản phẩm theo danh mục", detail = ex.Message });
      }
    }

    // 3. Chỉ định phương thức GET lấy chi tiết thông tin của duy nhất một sản phẩm theo ID khóa chính
    // Đường dẫn gọi dữ liệu: GET https://localhost:xxxx/api/products/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(int id)
    {
      try
      {
        // Quét bảng dữ liệu Products để tìm sản phẩm đầu tiên có mã ID khớp với tham số
        var product = await _context.Products
          .Include(p => p.CategoryProduct) // Tải kèm thông tin bảng danh mục liên kết
          .FirstOrDefaultAsync(p => p.Id == id); // Lấy bản ghi khớp đầu tiên hoặc null nếu không thấy

        // Xử lý kịch bản lỗi bảo vệ hệ thống: ID không tồn tại trong Database
        if (product == null)
        {
          // Trả về mã lỗi 404 kèm một gói tin JSON thông báo nhỏ gọn để Frontend tự xử lý UI
          return NotFound(new { message = "Không tìm thấy sản phẩm này trong hệ thống" });
        }

        // Trả về toàn bộ đối tượng sản phẩm (bao gồm cả trường Description chứa mô tả chất liệu) kèm mã 200 OK
        return Ok(product);
      }
      catch (Exception ex)
      {
        // Trả về lỗi 500 nếu sập kết nối SQL hoặc lỗi logic ngầm định
        return StatusCode(500, new { message = "Lỗi xử lý hệ thống khi lấy chi tiết sản phẩm", detail = ex.Message });
      }
    }
  }
}
