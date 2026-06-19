/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: Controller quản lý đơn hàng, gồm hiển thị danh sách, xem chi tiết, cập nhật trạng thái, và hủy đơn hàng.
*/

using CMS.Data;
using CMS.Data.Entities;
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

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
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

        // Action POST Edit chỉ cập nhật Trạng thái (Status) và Ghi chú (Notes) của đơn hàng.
        [HttpPost]
        public IActionResult Edit(Order model)
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
                // Nếu chuyển sang trạng thái "Đã hủy" (3) từ trạng thái khác
                if (newStatus == 3 && oldStatus != 3)
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
                // Nếu chuyển từ "Đã hủy" (3) quay lại các trạng thái hoạt động (0, 1, 2)
                else if (oldStatus == 3 && newStatus != 3)
                {
                    // Kiểm tra tồn kho trước khi trừ
                    if (existingOrder.OrderDetails != null)
                    {
                        foreach (var detail in existingOrder.OrderDetails)
                        {
                            var product = _context.Products.Find(detail.ProductId);
                            if (product == null)
                            {
                                ModelState.AddModelError("", $"Sản phẩm ID {detail.ProductId} không tồn tại.");
                                PrepareEditViewBag(existingOrder);
                                return View(existingOrder);
                            }
                            if (product.StockQuantity < detail.Quantity)
                            {
                                ModelState.AddModelError("", $"Sản phẩm '{product.Name}' không đủ số lượng trong kho để khôi phục đơn hàng (Còn tồn: {product.StockQuantity}).");
                                PrepareEditViewBag(existingOrder);
                                return View(existingOrder);
                            }
                        }

                        // Thực hiện trừ tồn kho
                        foreach (var detail in existingOrder.OrderDetails)
                        {
                            var product = _context.Products.Find(detail.ProductId);
                            if (product != null)
                            {
                                product.StockQuantity -= detail.Quantity;
                            }
                        }
                    }
                }
            }

            // Chỉ cập nhật Trạng thái và Ghi chú của đơn hàng
            existingOrder.Status = newStatus;
            existingOrder.Notes = model.Notes;

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Đơn hàng #{model.Id} đã được cập nhật trạng thái thành công!";
            return RedirectToAction("Index");
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

            // Chỉ cho phép hủy đơn hàng đang chờ duyệt (0) hoặc đang giao (1)
            if (order.Status == 2 || order.Status == 3)
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng đã hoàn thành hoặc đã hủy trước đó.";
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

            order.Status = 3; // 3: Đã hủy
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Đơn hàng #{id} đã được hủy thành công! Sản phẩm đã được hoàn trả lại kho.";
            return RedirectToAction("Index");
        }
    }
}
