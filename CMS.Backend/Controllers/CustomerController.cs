/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: Controller quản lý khách hàng, gồm hiển thị danh sách, thêm, sửa và xóa khách hàng.
*/

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // CustomerController xử lý các request bắt đầu bằng /Customer.
    [Authorize(Roles = "Admin,Staff")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiển thị danh sách tất cả khách hàng.
        public IActionResult Index()
        {
            var customers = _context.Customers
                .OrderBy(c => c.FullName)
                .ToList();

            return View(customers);
        }

        // Action Details hiển thị chi tiết một khách hàng và danh sách đơn hàng của họ.
        public IActionResult Details(int id)
        {
            var customer = _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefault(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // Action GET Create mở form thêm khách hàng mới.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Action POST Create lưu khách hàng mới vào database.
        [HttpPost]
        public IActionResult Create(Customer model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Customers.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Khách hàng đã được thêm thành công!";
            return RedirectToAction("Index");
        }

        // Action GET Edit mở form chỉnh sửa khách hàng.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var customer = _context.Customers.Find(id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // Action POST Edit cập nhật thông tin khách hàng vào database.
        [HttpPost]
        public IActionResult Edit(Customer model)
        {
            var existingCustomer = _context.Customers.AsNoTracking().FirstOrDefault(c => c.Id == model.Id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Customer.Orders));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Customers.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Thông tin khách hàng đã được cập nhật thành công!";
            return RedirectToAction("Index");
        }

        // Action Delete xóa khách hàng theo id.
        public IActionResult Delete(int id)
        {
            var customer = _context.Customers.Find(id);

            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Khách hàng đã được xóa thành công!";
            }

            return RedirectToAction("Index");
        }
    }
}
