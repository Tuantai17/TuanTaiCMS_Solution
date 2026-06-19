/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 18/6/2026
Mô tả: Controller quản trị Banner trong Admin panel, cho phép thêm, xóa, sửa, tải lên ảnh hoặc nhập link ảnh ngoài.
*/

using System;
using System.IO;
using System.Linq;
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class BannerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BannerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index: Hiển thị danh sách Banner sắp xếp theo thứ tự hiển thị tăng dần, ngày tạo giảm dần
        public IActionResult Index()
        {
            var banners = _context.Banners
                .OrderBy(b => b.DisplayOrder)
                .ThenByDescending(b => b.CreatedDate)
                .ToList();
            return View(banners);
        }

        // Action GET Create: Hiển thị form tạo mới banner
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Action POST Create: Nhận dữ liệu và lưu banner mới
        [HttpPost]
        public IActionResult Create(Banner model, IFormFile? uploadImage)
        {
            // Xử lý ảnh: Ưu tiên tải file lên trước, sau đó là nhập URL ảnh ngoài
            string? savedImagePath = SaveUploadImage(uploadImage);

            if (!string.IsNullOrEmpty(savedImagePath))
            {
                model.ImageUrl = savedImagePath;
            }
            else if (string.IsNullOrWhiteSpace(model.ImageUrl))
            {
                ModelState.AddModelError("ImageUrl", "Vui lòng chọn tải lên ảnh hoặc nhập URL hình ảnh.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CreatedDate = DateTime.Now;
            _context.Banners.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Banner đã được thêm thành công!";
            return RedirectToAction("Index");
        }

        // Action GET Edit: Hiển thị form sửa banner
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var banner = _context.Banners.Find(id);
            if (banner == null)
            {
                return NotFound();
            }
            return View(banner);
        }

        // Action POST Edit: Cập nhật banner
        [HttpPost]
        public IActionResult Edit(Banner model, IFormFile? uploadImage)
        {
            var existingBanner = _context.Banners.AsNoTracking().FirstOrDefault(b => b.Id == model.Id);
            if (existingBanner == null)
            {
                return NotFound();
            }

            string? savedImagePath = SaveUploadImage(uploadImage);

            if (!string.IsNullOrEmpty(savedImagePath))
            {
                model.ImageUrl = savedImagePath;
            }
            else if (string.IsNullOrWhiteSpace(model.ImageUrl))
            {
                // Nếu không tải ảnh mới và không nhập URL mới, giữ nguyên URL ảnh cũ
                model.ImageUrl = existingBanner.ImageUrl;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CreatedDate = existingBanner.CreatedDate;
            _context.Banners.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Banner đã được cập nhật thành công!";
            return RedirectToAction("Index");
        }

        // Action Delete: Xóa banner
        public IActionResult Delete(int id)
        {
            var banner = _context.Banners.Find(id);
            if (banner != null)
            {
                // Nếu ảnh được lưu local trong wwwroot/uploads, có thể xóa file vật lý đi (tùy chọn, để giữ sạch đĩa)
                if (banner.ImageUrl.StartsWith("/uploads/") && !banner.ImageUrl.StartsWith("/uploads/banner")) // Tránh xóa banner seeded mặc định
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", banner.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Lỗi khi xóa file ảnh banner cũ: " + ex.Message);
                        }
                    }
                }

                _context.Banners.Remove(banner);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Banner đã được xóa thành công!";
            }
            return RedirectToAction("Index");
        }

        // Action POST ToggleVisible: Bật/tắt trạng thái hiển thị (AJAX)
        [HttpPost]
        public IActionResult ToggleVisible(int id)
        {
            var banner = _context.Banners.Find(id);
            if (banner == null)
            {
                return Json(new { success = false, message = "Không tìm thấy banner này." });
            }

            banner.IsVisible = !banner.IsVisible;
            _context.SaveChanges();

            return Json(new { success = true, isVisible = banner.IsVisible });
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
