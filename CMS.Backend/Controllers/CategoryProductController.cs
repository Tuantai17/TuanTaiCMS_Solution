/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 22/5/2026
Mô tả: Controller quản lý loại sản phẩm, gồm hiển thị danh sách, thêm, sửa và xóa loại sản phẩm.
*/

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // CategoryProductController xử lý các request bắt đầu bằng /CategoryProduct.
    [Authorize(Roles = "Admin,Staff")]
    public class CategoryProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiển thị danh sách loại sản phẩm kèm số lượng sản phẩm thuộc mỗi loại.
        public IActionResult Index()
        {
            var categoriesProducts = _context.CategoriesProducts
                .Include(c => c.Products)
                .OrderBy(c => c.Name)
                .ToList();

            return View(categoriesProducts);
        }

        // Action GET Create mở form thêm loại sản phẩm mới.
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Action POST Create lưu loại sản phẩm mới vào database.
        [HttpPost]
        public IActionResult Create(CategoryProduct model)
        {
            ModelState.Remove(nameof(CategoryProduct.Products));

            var isNameDuplicated = _context.CategoriesProducts
                .Any(c => c.Name == model.Name);

            if (isNameDuplicated)
            {
                ModelState.AddModelError(nameof(CategoryProduct.Name), "Tên loại sản phẩm đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.CategoriesProducts.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Loại sản phẩm đã được thêm thành công!";
            return RedirectToAction("Index");
        }

        // Action GET Edit mở form chỉnh sửa loại sản phẩm theo id.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.CategoriesProducts.Find(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // Action POST Edit cập nhật loại sản phẩm vào database.
        [HttpPost]
        public IActionResult Edit(CategoryProduct model)
        {
            var existingCategory = _context.CategoriesProducts.AsNoTracking().FirstOrDefault(c => c.Id == model.Id);

            if (existingCategory == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(CategoryProduct.Products));

            var isNameDuplicated = _context.CategoriesProducts
                .Any(c => c.Id != model.Id && c.Name == model.Name);

            if (isNameDuplicated)
            {
                ModelState.AddModelError(nameof(CategoryProduct.Name), "Tên loại sản phẩm đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.CategoriesProducts.Update(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Loại sản phẩm đã được cập nhật thành công!";
            return RedirectToAction("Index");
        }

        // Action Delete xóa loại sản phẩm theo id.
        public IActionResult Delete(int id)
        {
            var category = _context.CategoriesProducts
                .Include(c => c.Products)
                .FirstOrDefault(c => c.Id == id);

            if (category != null)
            {
                if (category.Products != null && category.Products.Any())

                {
                    TempData["SuccessMessage"] = "Không thể xóa loại sản phẩm đang có sản phẩm. Vui lòng chuyển/xóa sản phẩm trước.";
                    return RedirectToAction("Index");
                }

                _context.CategoriesProducts.Remove(category);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Loại sản phẩm đã được xóa thành công!";
            }

            return RedirectToAction("Index");
        }
    }
}
