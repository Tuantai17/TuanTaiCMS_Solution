/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: Controller quản lý đơn hàng, gồm hiển thị danh sách, thêm, sửa, xóa và thay đổi trạng thái đơn hàng.
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
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiển thị danh sách tất cả đơn hàng kèm tên khách hàng.
        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // Action Details hiển thị chi tiết một đơn hàng kèm danh sách sản phẩm.
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

        // Action GET Create mở form thêm đơn hàng mới.
        [HttpGet]
        public IActionResult Create()
        {
            LoadCustomerList();
            return View();
        }

        // Action POST Create lưu đơn hàng mới vào database.
        [HttpPost]
        public IActionResult Create(Order model)
        {
            ModelState.Remove(nameof(Order.Customer));
            ModelState.Remove(nameof(Order.OrderDetails));

            if (!ModelState.IsValid)
            {
                LoadCustomerList(model.CustomerId);
                return View(model);
            }

            model.OrderDate = model.OrderDate == default ? DateTime.Now : model.OrderDate;

            _context.Orders.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đơn hàng đã được tạo thành công!";
            return RedirectToAction("Index");
        }

        // Action GET Edit mở form chỉnh sửa đơn hàng.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var order = _context.Orders.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            LoadCustomerList(order.CustomerId);
            return View(order);
        }

        // Action POST Edit cập nhật đơn hàng vào database.
        [HttpPost]
        public IActionResult Edit(Order model)
        {
            var existingOrder = _context.Orders.AsNoTracking().FirstOrDefault(o => o.Id == model.Id);

            if (existingOrder == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Order.Customer));
            ModelState.Remove(nameof(Order.OrderDetails));

            if (!ModelState.IsValid)
            {
                LoadCustomerList(model.CustomerId);
                return View(model);
            }

            model.OrderDate = model.OrderDate == default ? existingOrder.OrderDate : model.OrderDate;

            _context.Orders.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đơn hàng đã được cập nhật thành công!";
            return RedirectToAction("Index");
        }

        // Action Delete xóa đơn hàng theo id (bao gồm cả chi tiết đơn hàng).
        public IActionResult Delete(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.Id == id);

            if (order != null)
            {
                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    _context.OrderDetails.RemoveRange(order.OrderDetails);
                }
                _context.Orders.Remove(order);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Đơn hàng đã được xóa thành công!";
            }

            return RedirectToAction("Index");
        }

        // Hàm dùng chung để nạp dropdown khách hàng cho form Create/Edit.
        private void LoadCustomerList(int? selectedId = null)
        {
            ViewBag.CustomerList = new SelectList(
                _context.Customers.ToList(),
                "Id",
                "FullName",
                selectedId
            );
        }
    }
}
