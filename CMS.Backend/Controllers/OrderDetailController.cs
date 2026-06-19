/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: Controller quản lý chi tiết đơn hàng, gồm hiển thị danh sách, thêm, sửa và xóa chi tiết đơn hàng.
*/

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // OrderDetailController xử lý các request bắt đầu bằng /OrderDetail.
    [Authorize(Roles = "Admin,Staff")]
    public class OrderDetailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiển thị danh sách tất cả chi tiết đơn hàng kèm tên đơn hàng và sản phẩm.
        public IActionResult Index(int? orderId)
        {
            var query = _context.OrderDetails
                .Include(od => od.Order)
                    .ThenInclude(o => o!.Customer)
                .Include(od => od.Product)
                .AsQueryable();

            // Nếu có orderId thì lọc theo đơn hàng đó.
            if (orderId.HasValue)
            {
                query = query.Where(od => od.OrderId == orderId.Value);
                ViewBag.FilterOrderId = orderId.Value;
            }

            var orderDetails = query
                .OrderBy(od => od.OrderId)
                .ToList();

            return View(orderDetails);
        }

        // Action GET Create mở form thêm chi tiết đơn hàng.
        [HttpGet]
        public IActionResult Create(int? orderId)
        {
            LoadDropdowns(orderId);
            var model = new OrderDetail();
            if (orderId.HasValue)
            {
                model.OrderId = orderId.Value;
            }
            return View(model);
        }

        // Action POST Create lưu chi tiết đơn hàng mới vào database.
        [HttpPost]
        public IActionResult Create(OrderDetail model)
        {
            ModelState.Remove(nameof(OrderDetail.Order));
            ModelState.Remove(nameof(OrderDetail.Product));

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.OrderId);
                return View(model);
            }

            _context.OrderDetails.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Chi tiết đơn hàng đã được thêm thành công!";
            return RedirectToAction("Index");
        }

        // Action GET Edit mở form chỉnh sửa chi tiết đơn hàng.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var orderDetail = _context.OrderDetails.Find(id);

            if (orderDetail == null)
            {
                return NotFound();
            }

            LoadDropdowns(orderDetail.OrderId, orderDetail.ProductId);
            return View(orderDetail);
        }

        // Action POST Edit cập nhật chi tiết đơn hàng vào database.
        [HttpPost]
        public IActionResult Edit(OrderDetail model)
        {
            ModelState.Remove(nameof(OrderDetail.Order));
            ModelState.Remove(nameof(OrderDetail.Product));

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model.OrderId, model.ProductId);
                return View(model);
            }

            _context.OrderDetails.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Chi tiết đơn hàng đã được cập nhật thành công!";
            return RedirectToAction("Index");
        }

        // Action Delete xóa chi tiết đơn hàng theo id.
        public IActionResult Delete(int id)
        {
            var orderDetail = _context.OrderDetails.Find(id);

            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Chi tiết đơn hàng đã được xóa thành công!";
            }

            return RedirectToAction("Index");
        }

        // Hàm dùng chung để nạp dropdown đơn hàng và sản phẩm cho form Create/Edit.
        private void LoadDropdowns(int? selectedOrderId = null, int? selectedProductId = null)
        {
            // Hiển thị đơn hàng kèm tên khách hàng cho dễ nhận biết.
            var orders = _context.Orders
                .Include(o => o.Customer)
                .ToList()
                .Select(o => new { o.Id, Display = $"ĐH#{o.Id} - {o.Customer?.FullName ?? "?"} ({o.OrderDate:dd/MM/yyyy})" });

            ViewBag.OrderList = new SelectList(orders, "Id", "Display", selectedOrderId);
            ViewBag.ProductList = new SelectList(_context.Products.ToList(), "Id", "Name", selectedProductId);
        }
    }
}
