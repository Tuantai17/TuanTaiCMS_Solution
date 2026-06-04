/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: Controller quản lý sản phẩm, gồm hiển thị danh sách, thêm, sửa, xóa và upload ảnh sản phẩm.
*/

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // ProductController xử lý các request bắt đầu bằng /Product.
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiển thị danh sách tất cả sản phẩm kèm loại sản phẩm.
        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.CategoryProduct)
                .OrderBy(p => p.Name)
                .ToList();

            return View(products);
        }

        // Action Details hiển thị chi tiết một sản phẩm theo id.
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.CategoryProduct)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // Action GET Create mở form thêm sản phẩm mới.
        [HttpGet]
        public IActionResult Create()
        {
            LoadCategoryProductList();
            return View();
        }

        // Action POST Create lưu sản phẩm mới, xử lý upload ảnh nếu có.
        [HttpPost]
        public IActionResult Create(Product model, IFormFile? uploadImage)
        {
            ModelState.Remove(nameof(Product.CategoryProduct));

            if (!ModelState.IsValid)
            {
                LoadCategoryProductList(model.CategoryProductId);
                return View(model);
            }

            model.ImageUrl = SaveUploadImage(uploadImage) ?? model.ImageUrl;

            _context.Products.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Sản phẩm đã được thêm thành công!";
            return RedirectToAction("Index");
        }

        // Action GET Edit mở form chỉnh sửa sản phẩm.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            LoadCategoryProductList(product.CategoryProductId);
            return View(product);
        }

        // Action POST Edit cập nhật sản phẩm, giữ nguyên ảnh cũ nếu không upload ảnh mới.
        [HttpPost]
        public IActionResult Edit(Product model, IFormFile? uploadImage)
        {
            var existingProduct = _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == model.Id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Product.CategoryProduct));

            if (!ModelState.IsValid)
            {
                LoadCategoryProductList(model.CategoryProductId);
                model.ImageUrl = existingProduct.ImageUrl;
                return View(model);
            }

            model.ImageUrl = SaveUploadImage(uploadImage) ?? existingProduct.ImageUrl;

            _context.Products.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công!";
            return RedirectToAction("Index");
        }

        // Action Delete xóa sản phẩm theo id.
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công!";
            }

            return RedirectToAction("Index");
        }

        // Hàm dùng chung để nạp dropdown loại sản phẩm cho form Create/Edit.
        private void LoadCategoryProductList(int? selectedId = null)
        {
            ViewBag.CategoryProductList = new SelectList(
                _context.CategoriesProducts.ToList(),
                "Id",
                "Name",
                selectedId
            );
        }

        // Hàm lưu ảnh upload vào wwwroot/uploads và trả về đường dẫn tương đối.
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
