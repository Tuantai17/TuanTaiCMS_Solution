/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: Controller quản lý đơn hàng, gồm hiển thị danh sách, xem chi tiết, cập nhật trạng thái, và hủy đơn hàng.
*/

using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Models;
using CMS.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // OrderController xử lý các request bắt đầu bằng /Order.
    [Authorize(Roles = "Admin,Staff")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            ApplicationDbContext context,
            IEmailService emailService,
            INotificationService notificationService,
            ILogger<OrderController> logger)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
            _logger = logger;
        }

        // Action Index hiển thị danh sách tất cả đơn hàng kèm tên khách hàng và tổng tiền.
        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // Action Details hiển thị chi tiết một đơn hàng kèm thông tin khách hàng đầy đủ và danh sách sản phẩm.
        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Action GET Edit mở form cập nhật trạng thái đơn hàng.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Truyền thông tin khách hàng sang View để hiển thị (chỉ đọc)
            ViewBag.CustomerName = order.Customer?.FullName ?? "Không xác định";
            ViewBag.CustomerPhone = order.Customer?.Phone ?? "-";
            ViewBag.CustomerEmail = order.Customer?.Email ?? "-";

            return View(order);
        }

        // Action POST Edit cập nhật Trạng thái và Ghi chú của đơn hàng.
        [HttpPost]
        public async Task<IActionResult> Edit(Order model)
        {
            var existingOrder = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == model.Id);

            if (existingOrder == null)
            {
                return NotFound();
            }

            int oldStatus = existingOrder.Status;
            int newStatus = model.Status;

            if (oldStatus != newStatus)
            {
                // Kiểm tra Validation Quy trình
                bool isValidTransition = false;
                switch (oldStatus)
                {
                    case 0: // PENDING
                        isValidTransition = (newStatus == 1 || newStatus == 5);
                        break;
                    case 1: // CONFIRMED
                        isValidTransition = (newStatus == 2 || newStatus == 5);
                        break;
                    case 2: // PROCESSING
                        isValidTransition = (newStatus == 3 || newStatus == 5);
                        break;
                    case 3: // SHIPPING
                        isValidTransition = (newStatus == 4);
                        break;
                    case 4: // COMPLETED
                    case 5: // CANCELLED
                        isValidTransition = false; // Không được chuyển tiếp
                        break;
                }

                if (!isValidTransition)
                {
                    ModelState.AddModelError("", "Quy trình chuyển trạng thái không hợp lệ.");
                    PrepareEditViewBag(existingOrder);
                    return View(existingOrder);
                }

                // Nếu chuyển sang trạng thái "Đã hủy" (5) từ trạng thái khác
                if (newStatus == 5)
                {
                    // Hoàn trả tồn kho
                    if (existingOrder.OrderDetails != null)
                    {
                        foreach (var detail in existingOrder.OrderDetails)
                        {
                            var product = _context.Products.Find(detail.ProductId);
                            if (product != null)
                            {
                                product.StockQuantity += detail.Quantity;
                            }
                        }
                    }
                }

                // Lưu vết lịch sử thay đổi vào Ghi chú
                var oldStatusStr = GetStatusName(oldStatus);
                var newStatusStr = GetStatusName(newStatus);
                var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                string newNotes = model.Notes ?? "";
                newNotes += $"\n[{timestamp}] Trạng thái: {oldStatusStr} -> {newStatusStr}";
                
                existingOrder.Status = newStatus;
                existingOrder.Notes = newNotes;

                // Gui thong bao cho khach hang ve su thay doi trang thai
                if (existingOrder.CustomerId > 0)
                {
                    await _notificationService.CreateForCustomerAsync(
                        $"Đơn hàng #{existingOrder.Id} đã cập nhật",
                        $"Trạng thái đơn hàng của bạn đã chuyển sang: {newStatusStr}",
                        "OrderStatusUpdate",
                        existingOrder.CustomerId,
                        "Order",
                        existingOrder.Id
                    );
                }

                // Gui email khi chuyen trang thai sang Delivered (4)
                if (newStatus == 4 && existingOrder.DeliverySuccessEmailSentAt == null)
                {
                    existingOrder.DeliveredDate = DateTime.Now;
                    var customer = existingOrder.Customer;
                    if (customer != null && !string.IsNullOrWhiteSpace(customer.Email))
                    {
                        var orderCode = $"ORD-{existingOrder.Id:D6}";
                        var totalAmount = existingOrder.OrderDetails?.Sum(od => od.UnitPrice * od.Quantity) ?? 0;

                        var emailModel = new DeliverySuccessEmailModel
                        {
                            OrderId = existingOrder.Id,
                            OrderCode = orderCode,
                            CustomerName = customer.FullName,
                            CustomerEmail = customer.Email,
                            DeliveredDate = existingOrder.DeliveredDate,
                            Address = customer.Address,
                            TotalAmount = totalAmount
                        };

                        var emailLog = new EmailLog
                        {
                            EmailType = "DeliverySuccess",
                            RecipientEmail = customer.Email,
                            RecipientName = customer.FullName,
                            Subject = $"[TuanTaiCMS] Giao hang thanh cong - Don hang {orderCode}",
                            ReferenceType = "Order",
                            ReferenceId = existingOrder.Id,
                            Status = "Pending",
                            CreatedAt = DateTime.Now
                        };
                        _context.EmailLogs.Add(emailLog);

                        try
                        {
                            var sent = await _emailService.SendDeliverySuccessAsync(emailModel);
                            if (sent)
                            {
                                emailLog.Status = "Sent";
                                emailLog.SentAt = DateTime.Now;
                                existingOrder.DeliverySuccessEmailSentAt = DateTime.Now;
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
                            _logger.LogError(emailEx, "Loi gui email giao hang cho don #{Id}", existingOrder.Id);
                        }

                        await _notificationService.CreateForAllAdminsAsync(
                            $"Don hang #{existingOrder.Id} da giao thanh cong",
                            $"Don hang {orderCode} da duoc giao cho {customer.FullName}",
                            "DeliverySuccess", "Order", existingOrder.Id);
                    }
                }
            }
            else
            {
                // Chỉ cập nhật Ghi chú
                existingOrder.Notes = model.Notes;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Don hang #{model.Id} da duoc cap nhat trang thai thanh cong!";
            return RedirectToAction("Index");
        }

        private string GetStatusName(int status)
        {
            return status switch
            {
                0 => "Chờ duyệt",
                1 => "Đã duyệt",
                2 => "Đang chuẩn bị hàng",
                3 => "Đang giao hàng",
                4 => "Hoàn thành",
                5 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        private void PrepareEditViewBag(Order order)
        {
            ViewBag.CustomerName = order.Customer?.FullName ?? "Không xác định";
            ViewBag.CustomerPhone = order.Customer?.Phone ?? "-";
            ViewBag.CustomerEmail = order.Customer?.Email ?? "-";
        }

        // Action Cancel: Hủy đơn hàng và hoàn trả tồn kho sản phẩm.
        public IActionResult Cancel(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Chỉ cho phép hủy đơn hàng đang chờ duyệt (0), đã duyệt (1), đang chuẩn bị (2)
            if (order.Status >= 3)
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng đang giao, đã hoàn thành hoặc đã hủy trước đó.";
                return RedirectToAction("Index");
            }

            // Hoàn trả số lượng tồn kho sản phẩm
            if (order.OrderDetails != null && order.OrderDetails.Any())
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = _context.Products.Find(detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += detail.Quantity; // Cộng hoàn lại số lượng tồn kho
                    }
                }
            }

            var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            order.Notes = (order.Notes ?? "") + $"\n[{timestamp}] Trạng thái: {GetStatusName(order.Status)} -> Đã hủy (Bởi Admin)";
            
            order.Status = 5; // 5: CANCELLED
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Đơn hàng #{id} đã được hủy thành công! Sản phẩm đã được hoàn trả lại kho.";
            return RedirectToAction("Index");
        }
    }
}
