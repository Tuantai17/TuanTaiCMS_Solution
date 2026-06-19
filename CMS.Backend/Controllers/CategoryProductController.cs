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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        // Action Index hiển thị danh sách loại sản phẩm kèm số lượng sản phẩm, sắp xếp dạng cây.
        public IActionResult Index()
        {
            var categoriesProducts = GetSortedCategories();
            return View(categoriesProducts);
        }

        // Action GET Create mở form thêm loại sản phẩm mới.
        [HttpGet]
        public IActionResult Create()
        {
            LoadParentCategoryList();
            return View();
        }

        // Action POST Create lưu loại sản phẩm mới vào database.
        [HttpPost]
        public IActionResult Create(CategoryProduct model, IFormFile? uploadImage)
        {
            ModelState.Remove(nameof(CategoryProduct.Products));
            ModelState.Remove(nameof(CategoryProduct.Parent));
            ModelState.Remove(nameof(CategoryProduct.Children));

            var isNameDuplicated = _context.CategoriesProducts
                .Any(c => c.Name == model.Name);

            if (isNameDuplicated)
            {
                ModelState.AddModelError(nameof(CategoryProduct.Name), "Tên loại sản phẩm đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                LoadParentCategoryList(null, model.ParentId);
                return View(model);
            }

            model.ImageUrl = SaveUploadImage(uploadImage);

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

            LoadParentCategoryList(id, category.ParentId);
            return View(category);
        }

        // Action POST Edit cập nhật loại sản phẩm vào database.
        [HttpPost]
        public IActionResult Edit(CategoryProduct model, IFormFile? uploadImage)
        {
            var existingCategory = _context.CategoriesProducts.AsNoTracking().FirstOrDefault(c => c.Id == model.Id);

            if (existingCategory == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(CategoryProduct.Products));
            ModelState.Remove(nameof(CategoryProduct.Parent));
            ModelState.Remove(nameof(CategoryProduct.Children));

            var isNameDuplicated = _context.CategoriesProducts
                .Any(c => c.Id != model.Id && c.Name == model.Name);

            if (isNameDuplicated)
            {
                ModelState.AddModelError(nameof(CategoryProduct.Name), "Tên loại sản phẩm đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                LoadParentCategoryList(model.Id, model.ParentId);
                model.ImageUrl = existingCategory.ImageUrl;
                return View(model);
            }

            model.ImageUrl = SaveUploadImage(uploadImage) ?? existingCategory.ImageUrl;

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
                    TempData["ErrorMessage"] = "Không thể xóa loại sản phẩm đang có sản phẩm. Vui lòng chuyển/xóa sản phẩm trước.";
                    return RedirectToAction("Index");
                }

                // Cập nhật lại các danh mục con: chuyển về cha của danh mục bị xóa
                var childCategories = _context.CategoriesProducts.Where(c => c.ParentId == id).ToList();
                foreach (var child in childCategories)
                {
                    child.ParentId = category.ParentId;
                }

                _context.CategoriesProducts.Remove(category);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Loại sản phẩm đã được xóa thành công!";
            }

            return RedirectToAction("Index");
        }

        // Action POST: Xóa nhiều loại sản phẩm đã chọn.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSelected(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một loại sản phẩm để xóa.";
                return RedirectToAction("Index");
            }

            var categories = _context.CategoriesProducts
                .Include(c => c.Products)
                .Where(c => ids.Contains(c.Id))
                .ToList();

            var categoriesWithProducts = categories.Where(c => c.Products != null && c.Products.Any()).ToList();
            if (categoriesWithProducts.Any())
            {
                var names = string.Join(", ", categoriesWithProducts.Select(c => c.Name));
                TempData["ErrorMessage"] = $"Không thể xóa loại sản phẩm có chứa sản phẩm: {names}. Vui lòng chuyển hoặc xóa sản phẩm trước.";
                return RedirectToAction("Index");
            }

            foreach (var category in categories)
            {
                var childCategories = _context.CategoriesProducts.Where(c => c.ParentId == category.Id).ToList();
                foreach (var child in childCategories)
                {
                    if (category.ParentId.HasValue && ids.Contains(category.ParentId.Value))
                    {
                        child.ParentId = null;
                    }
                    else
                    {
                        child.ParentId = category.ParentId;
                    }
                }
            }

            _context.CategoriesProducts.RemoveRange(categories);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Đã xóa thành công {categories.Count} loại sản phẩm.";
            return RedirectToAction("Index");
        }

        // Hàm nạp dropdown chọn danh mục cha (loại trừ chính nó để tránh đệ quy vòng)
        private void LoadParentCategoryList(int? currentCategoryId = null, int? selectedParentId = null)
        {
            var query = _context.CategoriesProducts.AsQueryable();
            if (currentCategoryId.HasValue)
            {
                query = query.Where(c => c.Id != currentCategoryId.Value);
            }

            ViewBag.ParentId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                query.OrderBy(c => c.Name).ToList(),
                "Id",
                "Name",
                selectedParentId
            );
        }

        // Hàm lưu ảnh upload vào wwwroot/uploads và trả về đường dẫn tương đối
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

        // Hàm đệ quy lấy danh sách loại sản phẩm đã sắp xếp theo cây phân cấp
        private List<CategoryProduct> GetSortedCategories()
        {
            var allCategories = _context.CategoriesProducts
                .Include(c => c.Products)
                .ToList();
            var sortedList = new List<CategoryProduct>();

            var rootCategories = allCategories
                .Where(c => c.ParentId == null || !allCategories.Any(p => p.Id == c.ParentId))
                .OrderBy(c => c.Name)
                .ToList();

            foreach (var root in rootCategories)
            {
                AddChildrenToList(root, allCategories, sortedList, 0);
            }

            return sortedList;
        }

        private void AddChildrenToList(CategoryProduct current, List<CategoryProduct> all, List<CategoryProduct> sorted, int depth)
        {
            current.Depth = depth;
            sorted.Add(current);

            var children = all.Where(c => c.ParentId == current.Id).OrderBy(c => c.Name).ToList();
            foreach (var child in children)
            {
                AddChildrenToList(child, all, sorted, depth + 1);
            }
        }
    }
}

