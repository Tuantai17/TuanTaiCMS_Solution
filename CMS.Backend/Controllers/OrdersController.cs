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
using CMS.Data.Entities;
using CMS.Backend.Helpers;
using System.Globalization;

namespace CMS.Backend.Controllers
{
  [Route("api/[controller]")] // Định nghĩa đường dẫn gọi API: api/Orders
  [ApiController] // Kích hoạt thuộc tính xác thực dữ liệu đầu vào tự động (Validation)
  public class OrdersController : ControllerBase // Kế thừa ControllerBase để tối ưu bộ nhớ cho API thuần dữ liệu JSON
  {
    private readonly ApplicationDbContext _context; // Khai báo đối tượng trung gian kết nối cơ sở dữ liệu SQL Server
    private readonly EmailHelper _emailHelper; // Khai báo đối tượng helper gửi email
    private readonly IConfiguration _configuration;

    // Hàm khởi tạo (Constructor): "Tiêm" (Inject) ngữ cảnh dữ liệu cơ sở dữ liệu vào Controller thông qua DI
    public OrdersController(ApplicationDbContext context, EmailHelper emailHelper, IConfiguration configuration)
    {
      _context = context; // Gán context được tiêm vào cho biến nội bộ sử dụng
      _emailHelper = emailHelper;
      _configuration = configuration;
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

        // Gửi email xác nhận đơn hàng bất đồng bộ
        var customer = await _context.Customers.FindAsync(input.CustomerId);
        if (customer != null && !string.IsNullOrWhiteSpace(customer.Email))
        {
            var orderDetailsList = await _context.OrderDetails
                .Where(od => od.OrderId == newOrder.Id)
                .Include(od => od.Product)
                .ToListAsync();

            decimal totalAmount = 0;
            var itemsHtml = "";
            foreach (var detail in orderDetailsList)
            {
                var productName = detail.Product?.Name ?? "Sản phẩm";
                var qty = detail.Quantity;
                var price = detail.UnitPrice;
                var subTotal = qty * price;
                totalAmount += subTotal;
                itemsHtml += $"<tr><td style='border: 1px solid #ddd; padding: 8px;'>{productName}</td><td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{qty}</td><td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{price:N0}₫</td><td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{subTotal:N0}₫</td></tr>";
            }

            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; padding: 20px; border-radius: 10px;'>
                    <div style='text-align: center; border-bottom: 2px solid #CF102D; padding-bottom: 10px; margin-bottom: 20px;'>
                        <h2 style='color: #CF102D; margin: 0;'>MyKingdom - Xác Nhận Đơn Hàng</h2>
                    </div>
                    <p>Xin chào <strong>{customer.FullName}</strong>,</p>
                    <p>Cảm ơn bạn đã đặt mua sản phẩm tại <strong>Vương Quốc Đồ Chơi MyKingdom</strong>. Đơn hàng của bạn đã được tiếp nhận thành công và đang chờ xử lý.</p>
                    
                    <h3 style='color: #002664; border-bottom: 1px solid #eee; padding-bottom: 5px;'>Thông tin đơn hàng #{newOrder.Id}</h3>
                    <p><strong>Ngày đặt hàng:</strong> {newOrder.OrderDate:dd/MM/yyyy HH:mm}</p>
                    <p><strong>Trạng thái:</strong> Chờ duyệt</p>
                    {(string.IsNullOrWhiteSpace(newOrder.Notes) ? "" : $"<p><strong>Ghi chú:</strong> {newOrder.Notes}</p>")}

                    <h3 style='color: #002664; border-bottom: 1px solid #eee; padding-bottom: 5px;'>Chi tiết sản phẩm</h3>
                    <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                        <thead>
                            <tr style='background-color: #f2f2f2;'>
                                <th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Tên sản phẩm</th>
                                <th style='border: 1px solid #ddd; padding: 8px; text-align: center; width: 80px;'>SL</th>
                                <th style='border: 1px solid #ddd; padding: 8px; text-align: right; width: 100px;'>Đơn giá</th>
                                <th style='border: 1px solid #ddd; padding: 8px; text-align: right; width: 120px;'>Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            {itemsHtml}
                        </tbody>
                        <tfoot>
                            <tr>
                                <td colspan='3' style='border: 1px solid #ddd; padding: 8px; text-align: right; font-weight: bold;'>Tổng tiền thanh toán:</td>
                                <td style='border: 1px solid #ddd; padding: 8px; text-align: right; font-weight: bold; color: #CF102D;'>{totalAmount:N0}₫</td>
                            </tr>
                        </tfoot>
                    </table>

                    <p style='font-size: 0.9em; color: #666; text-align: center; border-top: 1px solid #eee; padding-top: 15px; margin-top: 25px;'>
                        Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ tổng đài hỗ trợ <strong>1900 1208</strong> hoặc phản hồi email này.<br/>
                        Chúc bạn và gia đình có những giây phút vui chơi tuyệt vời!
                    </p>
                </div>
            ";

            try
            {
                _ = Task.Run(async () => {
                    try
                    {
                        await _emailHelper.SendEmailAsync(customer.Email, $"[MyKingdom] Xác nhận đơn đặt hàng #{newOrder.Id} thành công", htmlBody);
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($">>> Lỗi gửi thư: {emailEx.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> Lỗi khi kích hoạt luồng gửi mail: {ex.Message}");
            }
        }

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

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders(
      [FromQuery] int? status,
      [FromQuery] string? keyword,
      [FromQuery] DateTime? fromDate,
      [FromQuery] DateTime? toDate,
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10)
    {
      if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
      {
        return authError!;
      }

      if (page <= 0)
      {
        page = 1;
      }

      pageSize = Math.Clamp(pageSize, 1, 20);

      if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
      {
        return BadRequest(new { message = "Ngày bắt đầu không được lớn hơn ngày kết thúc." });
      }

      try
      {
        var query = _context.Orders
          .AsNoTracking()
          .Where(o => o.CustomerId == customerId);

        if (status.HasValue)
        {
          query = query.Where(o => o.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
          var startDate = fromDate.Value.Date;
          query = query.Where(o => o.OrderDate >= startDate);
        }

        if (toDate.HasValue)
        {
          var endDateExclusive = toDate.Value.Date.AddDays(1);
          query = query.Where(o => o.OrderDate < endDateExclusive);
        }

        var normalizedKeyword = NormalizeOrderKeyword(keyword);
        if (!string.IsNullOrWhiteSpace(normalizedKeyword))
        {
          query = query.Where(o => o.Id.ToString().Contains(normalizedKeyword));
        }

        var totalItems = await query.CountAsync();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
          .OrderByDescending(o => o.OrderDate)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .Select(o => new OrderHistoryItemDto
          {
            Id = o.Id,
            OrderDate = o.OrderDate,
            Status = o.Status,
            PaymentMethod = null,
            TotalAmount = o.OrderDetails != null
              ? o.OrderDetails.Sum(od => od.UnitPrice * od.Quantity)
              : 0,
            TotalQuantity = o.OrderDetails != null
              ? o.OrderDetails.Sum(od => od.Quantity)
              : 0,
            ProductCount = o.OrderDetails != null
              ? o.OrderDetails.Count()
              : 0,
            FirstProductName = o.OrderDetails != null
              ? o.OrderDetails
                .OrderBy(od => od.Id)
                .Select(od => od.Product != null ? od.Product.Name : "Sản phẩm không xác định")
                .FirstOrDefault()
              : null,
            FirstProductImageUrl = o.OrderDetails != null
              ? o.OrderDetails
                .OrderBy(od => od.Id)
                .Select(od => od.Product != null ? od.Product.ImageUrl : null)
                .FirstOrDefault()
              : null,
            Notes = o.Notes
          })
          .ToListAsync();

        return Ok(new OrderHistoryListResponseDto
        {
          Items = items,
          Page = page,
          PageSize = pageSize,
          TotalItems = totalItems,
          TotalPages = totalPages
        });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Không thể tải lịch sử mua hàng. Vui lòng thử lại.", detail = ex.Message });
      }
    }

    [HttpGet("my/{id:int}")]
    public async Task<IActionResult> GetMyOrderDetail(int id)
    {
      if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
      {
        return authError!;
      }

      try
      {
        var order = await _context.Orders
          .AsNoTracking()
          .Where(o => o.Id == id && o.CustomerId == customerId)
          .Select(o => new OrderDetailResponseDto
          {
            Id = o.Id,
            OrderDate = o.OrderDate,
            Status = o.Status,
            Notes = o.Notes,
            PaymentMethod = null,
            TotalAmount = o.OrderDetails != null
              ? o.OrderDetails.Sum(od => od.UnitPrice * od.Quantity)
              : 0,
            Items = o.OrderDetails != null
              ? o.OrderDetails
                .OrderBy(od => od.Id)
                .Select(od => new OrderDetailItemDto
                {
                  Id = od.Id,
                  ProductId = od.ProductId,
                  ProductName = od.Product != null ? od.Product.Name : "Sản phẩm không xác định",
                  ProductImageUrl = od.Product != null ? od.Product.ImageUrl : null,
                  Quantity = od.Quantity,
                  UnitPrice = od.UnitPrice,
                  LineTotal = od.UnitPrice * od.Quantity
                })
                .ToList()
              : new List<OrderDetailItemDto>()
          })
          .FirstOrDefaultAsync();

        if (order == null)
        {
          return NotFound(new { message = "Không tìm thấy đơn hàng." });
        }

        return Ok(order);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Không thể tải chi tiết đơn hàng. Vui lòng thử lại.", detail = ex.Message });
      }
    }

    private bool TryGetAuthenticatedCustomerId(out int customerId, out IActionResult? errorResult)
    {
      customerId = 0;
      errorResult = null;

      var authorizationHeader = Request.Headers.Authorization.FirstOrDefault();
      if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
      {
        errorResult = Unauthorized(new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
        return false;
      }

      var token = authorizationHeader["Bearer ".Length..].Trim();
      var secret = _configuration["CustomerSession:Secret"] ?? "TuanTaiCMS.CustomerSession.Secret.2026";
      if (!CustomerSessionTokenHelper.TryValidateToken(token, secret, out customerId))
      {
        errorResult = Unauthorized(new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
        return false;
      }

      return true;
    }

    private static string? NormalizeOrderKeyword(string? keyword)
    {
      if (string.IsNullOrWhiteSpace(keyword))
      {
        return null;
      }

      var trimmed = keyword.Trim();
      var digitsOnly = new string(trimmed.Where(char.IsDigit).ToArray());

      if (trimmed.StartsWith("MKD", true, CultureInfo.InvariantCulture) && digitsOnly.Length > 0)
      {
        return digitsOnly.TrimStart('0');
      }

      return digitsOnly.Length > 0 ? digitsOnly : trimmed;
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

  public class OrderHistoryListResponseDto
  {
    public List<OrderHistoryItemDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
  }

  public class OrderHistoryItemDto
  {
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public int Status { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalQuantity { get; set; }
    public int ProductCount { get; set; }
    public string? FirstProductName { get; set; }
    public string? FirstProductImageUrl { get; set; }
    public string? Notes { get; set; }
  }

  public class OrderDetailResponseDto
  {
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public int Status { get; set; }
    public string? Notes { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderDetailItemDto> Items { get; set; } = new();
  }

  public class OrderDetailItemDto
  {
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
  }
}
