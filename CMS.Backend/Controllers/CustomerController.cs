/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: Controller quản lý khách hàng, gồm hiển thị danh sách, thêm, sửa và xóa khách hàng.
*/

using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Http;

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
                .Include(c => c.CustomerAddresses)
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
        public IActionResult Create(Customer model, IFormFile? uploadImage)
        {
            ModelState.Remove(nameof(Customer.Orders));
            ModelState.Remove(nameof(Customer.CustomerAddresses));

            // Xử lý ảnh đại diện nếu có tải lên
            string? savedImagePath = SaveUploadImage(uploadImage);
            if (!string.IsNullOrEmpty(savedImagePath))
            {
                model.AvatarUrl = savedImagePath;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Mã hóa mật khẩu bằng BCrypt trước khi lưu vào database
            model.Password = PasswordHelper.HashPassword(model.Password);
            model.CreatedAt = DateTime.Now;

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
        public IActionResult Edit(Customer model, string? NewPassword, IFormFile? uploadImage)
        {
            var existingCustomer = _context.Customers.AsNoTracking().FirstOrDefault(c => c.Id == model.Id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Customer.Orders));
            ModelState.Remove(nameof(Customer.CustomerAddresses));
            ModelState.Remove(nameof(Customer.Password));

            // Xử lý ảnh đại diện
            string? savedImagePath = SaveUploadImage(uploadImage);
            if (!string.IsNullOrEmpty(savedImagePath))
            {
                model.AvatarUrl = savedImagePath;
            }
            else
            {
                model.AvatarUrl = existingCustomer.AvatarUrl;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Xử lý mật khẩu: nếu có nhập mới thì hash, nếu không thì giữ nguyên cũ
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                model.Password = PasswordHelper.HashPassword(NewPassword);
            }
            else
            {
                model.Password = existingCustomer.Password;
            }

            model.CreatedAt = existingCustomer.CreatedAt;

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

        // Helper lưu ảnh upload vào wwwroot/uploads
        private string? SaveUploadImage(IFormFile? uploadImage)
        {
            if (uploadImage == null || uploadImage.Length == 0)
            {
                return null;
            }

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var fileName = Guid.NewGuid() + Path.GetExtension(uploadImage.FileName);
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                uploadImage.CopyTo(stream);
            }

            return "/uploads/" + fileName;
        }
    }
}
