/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: API Controller quản lý đơn đặt hàng trực tiếp từ giỏ hàng Frontend, cung cấp phương thức POST để chèn dữ liệu vào Database.
*/

using Microsoft.AspNetCore.Mvc; // Import thư viện hỗ trợ xây dựng các API Controller của ASP.NET Core
using Microsoft.EntityFrameworkCore; // Import thư viện hỗ trợ truy vấn Database bất đồng bộ và load quan hệ
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
        return BadRequest(new { message = "Dữ liệu đơn hàng không hợp lệ hoặc trống rỗng" });
      }

      // 2. Kiểm tra nếu mã khách hàng không hợp lệ
      if (input.CustomerId <= 0)
      {
        return BadRequest(new { message = "Mã khách hàng CustomerId không hợp lệ" });
      }

      // 3. Kiểm tra giỏ hàng trống
      if (input.CartItems == null || input.CartItems.Count == 0)
      {
        return BadRequest(new { message = "Giỏ hàng rỗng. Vui lòng thêm sản phẩm trước khi đặt hàng!" });
      }

      // Bắt đầu Transaction để đảm bảo tính nhất quán của dữ liệu (nếu thêm đơn hàng thành công nhưng trừ tồn kho lỗi thì rollback toàn bộ)
      using var transaction = await _context.Database.BeginTransactionAsync();

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

        // Thêm đối tượng đơn hàng mới vào DbSet Orders tạm thời
        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync(); // Lưu trước để phát sinh ID đơn hàng (newOrder.Id)

        // Bước B: Duyệt qua mảng giỏ hàng, nạp vào bảng OrderDetails và trừ số lượng tồn kho Product
        foreach (var cartItem in input.CartItems)
        {
          // Truy vấn tìm sản phẩm
          var product = await _context.Products.FindAsync(cartItem.ProductId);
          if (product == null)
          {
            await transaction.RollbackAsync();
            return BadRequest(new { message = $"Sản phẩm có mã ID {cartItem.ProductId} không tồn tại trong hệ thống." });
          }

          // Kiểm tra số lượng tồn kho
          if (product.StockQuantity < cartItem.Quantity)
          {
            await transaction.RollbackAsync();
            return BadRequest(new { message = $"Sản phẩm '{product.Name}' không đủ số lượng trong kho (Còn tồn: {product.StockQuantity}). Vui lòng điều chỉnh lại giỏ hàng!" });
          }

          // Khấu trừ số lượng tồn kho
          product.StockQuantity -= cartItem.Quantity;

          // Tạo chi tiết đơn hàng
          var orderDetail = new OrderDetail
          {
            OrderId = newOrder.Id,
            ProductId = cartItem.ProductId,
            Quantity = cartItem.Quantity,
            UnitPrice = product.Price // Lấy đơn giá thật của sản phẩm tại thời điểm mua gán vào UnitPrice
          };

          _context.OrderDetails.Add(orderDetail);
        }

        // Lưu toàn bộ chi tiết đơn hàng và cập nhật tồn kho sản phẩm xuống SQL Server
        await _context.SaveChangesAsync();

        // Chốt và commit giao dịch thành công
        await transaction.CommitAsync();

        return StatusCode(201, new
        {
          message = "Đặt hàng thành công!",
          orderId = newOrder.Id
        });
      }
      catch (Exception ex)
      {
        // Có lỗi xảy ra, tiến hành hoàn tác dữ liệu
        await transaction.RollbackAsync();
        return StatusCode(500, new
        {
          message = "Lỗi xử lý tạo đơn hàng ngầm bên phía Server",
          detail = ex.Message
        });
      }
    }

    /// <summary>
    /// API: Lấy lịch sử đơn hàng của một khách hàng cụ thể (Giao thức GET)
    /// Đường dẫn gọi dữ liệu: GET https://localhost:xxxx/api/Orders/customer/{customerId}
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
      try
      {
        var orders = await _context.Orders
          .Where(o => o.CustomerId == customerId)
          .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
          .OrderByDescending(o => o.OrderDate)
          .Select(o => new
          {
            o.Id,
            o.OrderDate,
            o.Status,
            o.Notes,
            OrderDetails = o.OrderDetails.Select(od => new
            {
              od.Id,
              od.ProductId,
              od.Quantity,
              od.UnitPrice,
              ProductName = od.Product != null ? od.Product.Name : "Không xác định",
              ProductImageUrl = od.Product != null ? od.Product.ImageUrl : ""
            })
          })
          .ToListAsync();

        return Ok(orders);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Lỗi hệ thống khi tải lịch sử đơn hàng", detail = ex.Message });
      }
    }
  }

  // LỚP DTO TRUNG GIAN ĐỂ HỨNG DỮ LIỆU TỪ GIỎ HÀNG FRONTEND TRUYỀN LÊN TRONG THÂN REQUEST
  public class OrderInputDTO
  {
    public int CustomerId { get; set; } // Mã định danh khách hàng đặt mua
    public string? Notes { get; set; } // Ghi chú đơn hàng (ví dụ: giao giờ hành chính, đóng gói kỹ...)
    public List<CartItemInputDTO> CartItems { get; set; } = new List<CartItemInputDTO>(); // Mảng sản phẩm mua
  }

  public class CartItemInputDTO
  {
    public int ProductId { get; set; }
    public int Quantity { get; set; }
  }
}
