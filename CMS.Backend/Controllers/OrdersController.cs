/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: API Controller quản lý đơn đặt hàng trực tiếp từ giỏ hàng Frontend, cung cấp phương thức POST để chèn dữ liệu vào Database.
*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using CMS.Data.Enums;
using CMS.Backend.Helpers;
using CMS.Backend.Models;
using CMS.Backend.Services;
using System.Globalization;

namespace CMS.Backend.Controllers
{
  [Route("api/[controller]")] // Định nghĩa đường dẫn gọi API: api/Orders
  [ApiController] // Kích hoạt thuộc tính xác thực dữ liệu đầu vào tự động (Validation)
  public class OrdersController : ControllerBase
  {
    private readonly ApplicationDbContext _context;
    private readonly EmailHelper _emailHelper;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
      ApplicationDbContext context,
      EmailHelper emailHelper,
      IEmailService emailService,
      INotificationService notificationService,
      IConfiguration configuration,
      ILogger<OrdersController> logger)
    {
      _context = context;
      _emailHelper = emailHelper;
      _emailService = emailService;
      _notificationService = notificationService;
      _configuration = configuration;
      _logger = logger;
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

        // Luu toan bo chi tiet don hang va cap nhat ton kho san pham xuong SQL Server
        await _context.SaveChangesAsync();

        // Chot va commit giao dich thanh cong
        await transaction.CommitAsync();

        // Luu PaymentMethod tu Notes (parse tu Frontend)
        var orderCode = $"ORD-{newOrder.Id:D6}";

        // Gui email xac nhan don hang bat dong bo
        bool emailSent = false;
        var customer = await _context.Customers.FindAsync(input.CustomerId);
        if (customer != null && !string.IsNullOrWhiteSpace(customer.Email) && newOrder.OrderConfirmationEmailSentAt == null)
        {
            var orderDetailsList = await _context.OrderDetails
                .Where(od => od.OrderId == newOrder.Id)
                .Include(od => od.Product)
                .ToListAsync();

            decimal totalAmount = orderDetailsList.Sum(d => d.Quantity * d.UnitPrice);

            var emailModel = new OrderEmailModel
            {
                OrderId = newOrder.Id,
                OrderCode = orderCode,
                CustomerName = customer.FullName,
                CustomerEmail = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                OrderDate = newOrder.OrderDate,
                PaymentMethod = newOrder.PaymentMethod ?? "COD",
                PaymentStatus = "Cho thanh toan",
                OrderStatus = "Cho duyet",
                TotalAmount = totalAmount,
                Items = orderDetailsList.Select(d => new OrderEmailItemModel
                {
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? "San pham",
                    ProductImage = d.Product?.ImageUrl,
                    UnitPrice = d.UnitPrice,
                    Quantity = d.Quantity,
                    LineTotal = d.UnitPrice * d.Quantity
                }).ToList()
            };

            // Tao EmailLog Pending
            var emailLog = new EmailLog
            {
                EmailType = "OrderConfirmation",
                RecipientEmail = customer.Email,
                RecipientName = customer.FullName,
                Subject = $"[TuanTaiCMS] Xac nhan don hang {orderCode}",
                ReferenceType = "Order",
                ReferenceId = newOrder.Id,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };
            _context.EmailLogs.Add(emailLog);
            await _context.SaveChangesAsync();

            try
            {
                emailSent = await _emailService.SendOrderConfirmationAsync(emailModel);
                if (emailSent)
                {
                    emailLog.Status = "Sent";
                    emailLog.SentAt = DateTime.Now;
                    newOrder.OrderConfirmationEmailSentAt = DateTime.Now;
                }
                else
                {
                    emailLog.Status = "Failed";
                    emailLog.ErrorMessage = "EmailService returned false";
                }
            }
            catch (Exception emailEx)
            {
                emailLog.Status = "Failed";
                emailLog.ErrorMessage = emailEx.Message.Length > 1000 ? emailEx.Message[..1000] : emailEx.Message;
                _logger.LogError(emailEx, "Loi gui email xac nhan don hang #{OrderId}", newOrder.Id);
            }

            await _context.SaveChangesAsync();

            // Tao Notification neu email that bai
            if (!emailSent)
            {
                await _notificationService.CreateForAllAdminsAsync(
                    $"Email xac nhan don hang #{newOrder.Id} that bai",
                    $"Khong gui duoc email xac nhan don hang {orderCode} cho {customer.Email}",
                    "EmailFailed", "EmailLog", emailLog.Id);
            }
        }

        // Tao thong bao don hang moi cho Admin
        await _notificationService.CreateForAllAdminsAsync(
            $"Don hang moi #{newOrder.Id}",
            $"Khach hang {customer?.FullName ?? "N/A"} vua dat don hang {orderCode}",
            "NewOrder", "Order", newOrder.Id);

        // Kiem tra ton kho thap
        foreach (var cartItem in input.CartItems)
        {
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product != null && product.StockQuantity <= 5)
            {
                await _notificationService.CreateForAllAdminsAsync(
                    $"San pham sap het hang: {product.Name}",
                    $"San pham '{product.Name}' chi con {product.StockQuantity} san pham trong kho.",
                    "LowStock", "Product", product.Id);
            }
        }

        return StatusCode(201, new
        {
          success = true,
          message = "Dat hang thanh cong!",
          orderId = newOrder.Id,
          orderCode = orderCode,
          emailSent = emailSent
        });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        return StatusCode(500, new
        {
          success = false,
          message = "Loi xu ly tao don hang ngam ben phia Server",
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

    /// <summary>
    /// API: Lấy tổng doanh thu toàn thời gian (từ các đơn hàng hoàn thành) (Giao thức GET)
    /// Đường dẫn gọi dữ liệu: GET https://localhost:xxxx/api/Orders/total-revenue
    /// </summary>
    [HttpGet("total-revenue")]
    public async Task<IActionResult> GetTotalRevenue()
    {
      try
      {
        var totalRevenue = await _context.OrderDetails
          .Where(od => od.Order != null && od.Order.Status == 4) // 4 = Hoàn thành
          .SumAsync(od => (decimal?)(od.Quantity * od.UnitPrice)) ?? 0;

        return Ok(new { TotalRevenue = totalRevenue });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Lỗi hệ thống khi tải tổng doanh thu", detail = ex.Message });
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
                  LineTotal = od.UnitPrice * od.Quantity,
                  CanReview = o.Status == (int)OrderStatus.COMPLETED && od.ProductReview == null,
                  HasReview = od.ProductReview != null,
                  ReviewId = od.ProductReview != null ? od.ProductReview.Id : null,
                  ReviewStatus = od.ProductReview != null ? od.ProductReview.Status : null
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

    [HttpPut("my/{id:int}/cancel")]
    public async Task<IActionResult> CancelMyOrder(int id, [FromBody] CancelOrderRequestDto request)
    {
      if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
      {
        return authError!;
      }

      using var transaction = await _context.Database.BeginTransactionAsync();
      try
      {
        var order = await _context.Orders
          .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
          .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId);

        if (order == null)
        {
          return NotFound(new { message = "Không tìm thấy đơn hàng." });
        }

        if (order.Status >= 2) // Allow cancelling when Pending (0) or Confirmed (1)
        {
            return BadRequest(new { message = "Không thể hủy đơn hàng lúc này vì đơn hàng đã được chuẩn bị hoặc đã giao." });
        }

        // Cancel order
        var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        order.Status = 5; // Cancelled
        order.Notes = (order.Notes ?? "") + $"\n[{timestamp}] Trạng thái: Chờ duyệt -> Đã hủy (Bởi Người dùng)\n[Lý do hủy: {request.Reason}]";

        // Restore inventory
        if (order.OrderDetails != null)
        {
          foreach (var detail in order.OrderDetails)
          {
            if (detail.Product != null)
            {
              detail.Product.StockQuantity += detail.Quantity;
            }
          }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new { message = "Hủy đơn hàng thành công." });
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        return StatusCode(500, new { message = "Không thể hủy đơn hàng. Vui lòng thử lại.", detail = ex.Message });
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
    public bool CanReview { get; set; }
    public bool HasReview { get; set; }
    public int? ReviewId { get; set; }
    public ReviewStatus? ReviewStatus { get; set; }
  }

  public class CancelOrderRequestDto
  {
    public string Reason { get; set; } = string.Empty;
  }
}
