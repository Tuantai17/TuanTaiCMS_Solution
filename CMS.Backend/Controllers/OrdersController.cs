/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: API Controller quản lý đơn đặt hàng trực tiếp từ giỏ hàng Frontend, cung cấp phương thức POST để chèn dữ liệu vào Database.
*/

using Microsoft.AspNetCore.Mvc; // Import thư viện hỗ trợ xây dựng các API Controller của ASP.NET Core
using CMS.Data; // Import namespace chứa lớp ngữ cảnh dữ liệu ApplicationDbContext
using CMS.Data.Entities; // Import namespace chứa các lớp thực thể Entity mẫu của Solution

namespace CMS.Backend.Controllers
{
  [Route("api/[controller]")] // Định nghĩa đường dẫn gọi API: api/Orders
  [ApiController] // Kích hoạt thuộc tính xác thực dữ liệu đầu vào tự động (Validation)
  public class OrdersController : ControllerBase // Kế thừa ControllerBase để tối ưu bộ nhớ cho API thuần dữ liệu JSON
  {
    private readonly ApplicationDbContext _context; // Khai báo đối tượng trung gian kết nối cơ sở dữ liệu SQL Server

    // Hàm khởi tạo (Constructor): "Tiêm" (Inject) ngữ cảnh dữ liệu cơ sở dữ liệu vào Controller thông qua DI
    public OrdersController(ApplicationDbContext context)
    {
      _context = context; // Gán context được tiêm vào cho biến nội bộ sử dụng
    }

    /// <summary>
    /// API: Tiếp nhận đơn đặt hàng từ giỏ hàng FrontEnd gửi lên (Giao thức POST)
    /// Đường dẫn gọi dữ liệu: POST https://localhost:xxxx/api/Orders
    /// </summary>
    /// <param name="input">Đối tượng chứa thông tin đặt hàng từ Frontend gửi lên trong Body của Request</param>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderInputDTO input)
    {
      // 1. Kiểm tra kịch bản lỗi bảo vệ hệ thống: Nếu dữ liệu truyền lên trống rỗng
      if (input == null)
      {
        // Trả về mã lỗi 400 Bad Request kèm thông báo dữ liệu không hợp lệ bằng JSON
        return BadRequest(new { message = "Dữ liệu đơn hàng không hợp lệ hoặc trống rỗng" });
      }

      // 2. Kiểm tra nếu mã khách hàng không hợp lệ (không lớn hơn 0)
      if (input.CustomerId <= 0)
      {
        // Trả về mã lỗi 400 Bad Request báo lỗi thông tin CustomerId
        return BadRequest(new { message = "Mã khách hàng CustomerId không hợp lệ" });
      }

      try
      {
        // Bước A: Tự động khởi tạo và gán giá trị cho đối tượng thực thể Đơn hàng (Order) mới
        var newOrder = new Order
        {
          OrderDate = DateTime.Now, // Tự động lấy ngày giờ thực tế của hệ thống máy chủ tại thời điểm đặt mua
          CustomerId = input.CustomerId, // Nhận thông tin ID khách hàng từ Frontend truyền lên
          Status = 0, // 0: Mặc định trạng thái đơn hàng mới là "Chờ duyệt / Chờ xử lý"
          Notes = input.Notes // Nhận ghi chú đơn hàng từ Frontend truyền lên
        };

        // Bước B: Thêm đối tượng đơn hàng mới vào DbSet Orders tạm thời
        _context.Orders.Add(newOrder);

        // Bước C: Chốt lưu toàn bộ các thay đổi xuống cơ sở dữ liệu SQL Server để đồng bộ
        await _context.SaveChangesAsync(); // Ép hệ thống sinh ra mã ID Đơn hàng tự động tăng và lưu lại

        // Bước D: Trả về mã thành công 201 Created và gửi ngược lại mã ID đơn hàng vừa tạo cùng thông báo
        return StatusCode(201, new
        {
          message = "Đặt hàng thành công!", // Lời nhắn thành công gửi lại Client
          orderId = newOrder.Id // Mã ID đơn hàng mới sinh ra dưới Database
        });
      }
      catch (Exception ex)
      {
        // Bảo vệ hệ thống: Trả về lỗi 500 nếu sập kết nối SQL hoặc gặp lỗi logic ngầm
        return StatusCode(500, new
        {
          message = "Lỗi xử lý tạo đơn hàng ngầm bên phía Server",
          detail = ex.Message
        });
      }
    }
  }

  // LỚP DTO TRUNG GIAN ĐỂ HỨNG DỮ LIỆU TỪ GIỎ HÀNG FRONTEND TRUYỀN LÊN TRONG THÂN REQUEST
  public class OrderInputDTO
  {
    public int CustomerId { get; set; } // Mã định danh khách hàng đặt mua
    public string? Notes { get; set; } // Ghi chú đơn hàng (ví dụ: giao giờ hành chính, đóng gói kỹ...)
  }
}
