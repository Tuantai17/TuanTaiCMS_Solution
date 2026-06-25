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
    [Authorize(Roles = "Admin,Staff")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index hiển thị danh sách sản phẩm có phân trang và bộ lọc danh mục.
        // categoryId: mã danh mục cần lọc (mặc định = null là không lọc)
        // page: trang hiện tại (mặc định = 1)
        // pageSize: số sản phẩm mỗi trang (mặc định = 10)
        public IActionResult Index(int? categoryId = null, int page = 1, int pageSize = 10)
        {
            // Đảm bảo giá trị hợp lệ
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Lọc sản phẩm theo CategoryProductId nếu có truyền categoryId
            IQueryable<Product> query = _context.Products.Include(p => p.CategoryProduct);
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryProductId == categoryId.Value);
            }

            // Tổng số sản phẩm trong database sau khi lọc
            var totalItems = query.Count();

            // Tổng số trang
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Đảm bảo trang hiện tại không vượt quá tổng số trang
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Truy vấn sản phẩm theo trang: bỏ qua (page-1)*pageSize và lấy pageSize bản ghi
            var products = query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Load danh sách danh mục để hiển thị trong dropdown filter
            LoadCategoryProductList(categoryId);

            // Truyền thông tin phân trang và lọc sang View qua ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            ViewBag.SelectedCategoryId = categoryId;

            return View(products);
        }

        // Action Details hiển thị chi tiết một sản phẩm theo id.
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.CategoryProduct)
                .Include(p => p.ProductImages)
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
        public IActionResult Create(Product model, IFormFile? uploadImage, List<IFormFile>? galleryImages)
        {
            ModelState.Remove(nameof(Product.CategoryProduct));
            ModelState.Remove(nameof(Product.ProductImages));

            if (!ModelState.IsValid)
            {
                LoadCategoryProductList(model.CategoryProductId);
                return View(model);
            }

            model.ImageUrl = SaveUploadImage(uploadImage) ?? model.ImageUrl;

            // Xử lý upload thêm nhiều ảnh chi tiết
            if (galleryImages != null && galleryImages.Count > 0)
            {
                foreach (var file in galleryImages)
                {
                    var path = SaveUploadImage(file);
                    if (!string.IsNullOrEmpty(path))
                    {
                        model.ProductImages.Add(new ProductImage { ImageUrl = path });
                    }
                }
            }

            _context.Products.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Sản phẩm đã được thêm thành công!";
            return RedirectToAction("Index");
        }

        // Action GET Edit mở form chỉnh sửa sản phẩm.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            LoadCategoryProductList(product.CategoryProductId);
            return View(product);
        }

        // Action POST Edit cập nhật sản phẩm, giữ nguyên ảnh cũ nếu không upload ảnh mới.
        [HttpPost]
        public IActionResult Edit(Product model, IFormFile? uploadImage, List<IFormFile>? galleryImages, List<int>? deleteGalleryImages)
        {
            var existingProduct = _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.Id == model.Id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Product.CategoryProduct));
            ModelState.Remove(nameof(Product.ProductImages));

            if (!ModelState.IsValid)
            {
                LoadCategoryProductList(model.CategoryProductId);
                model.ImageUrl = existingProduct.ImageUrl;
                model.ProductImages = existingProduct.ProductImages;
                return View(model);
            }

            existingProduct.Name = model.Name;
            existingProduct.Description = model.Description;
            existingProduct.Price = model.Price;
            existingProduct.StockQuantity = model.StockQuantity;
            existingProduct.CategoryProductId = model.CategoryProductId;
            
            // Cập nhật các trường trạng thái và giá sale
            existingProduct.IsNew = model.IsNew;
            existingProduct.IsBestSelling = model.IsBestSelling;
            existingProduct.IsSale = model.IsSale;
            existingProduct.SalePrice = model.IsSale ? model.SalePrice : 0;
            existingProduct.DisplayOrderNew = model.DisplayOrderNew;
            existingProduct.DisplayOrderSale = model.DisplayOrderSale;
            existingProduct.DisplayOrderBestSelling = model.DisplayOrderBestSelling;

            var newImageUrl = SaveUploadImage(uploadImage);
            if (!string.IsNullOrWhiteSpace(newImageUrl))
            {
                existingProduct.ImageUrl = newImageUrl;
            }

            // Xóa các ảnh chi tiết được tích chọn xóa
            if (deleteGalleryImages != null && deleteGalleryImages.Count > 0)
            {
                var imgsToDelete = _context.ProductImages
                    .Where(pi => deleteGalleryImages.Contains(pi.Id))
                    .ToList();
                _context.ProductImages.RemoveRange(imgsToDelete);
            }

            // Thêm ảnh chi tiết mới tải lên
            if (galleryImages != null && galleryImages.Count > 0)
            {
                foreach (var file in galleryImages)
                {
                    var path = SaveUploadImage(file);
                    if (!string.IsNullOrEmpty(path))
                    {
                        existingProduct.ProductImages.Add(new ProductImage { ImageUrl = path });
                    }
                }
            }

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

        // Action AJAX: Bật/tắt trạng thái New cho sản phẩm.
        [HttpPost]
        public IActionResult ToggleNew(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            product.IsNew = !product.IsNew;
            _context.SaveChanges();

            return Json(new { success = true, isNew = product.IsNew });
        }

        // Action AJAX: Bật/tắt trạng thái Sale cho sản phẩm.
        // Khi bật Sale: truyền kèm salePrice (giá khuyến mãi).
        // Khi tắt Sale: salePrice = 0.
        [HttpPost]
        public IActionResult ToggleSale(int id, decimal salePrice)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            if (salePrice > 0)
            {
                // Bật Sale: validate giá sale phải nhỏ hơn giá gốc
                if (salePrice >= product.Price)
                {
                    return Json(new { success = false, message = "Giá sale phải nhỏ hơn giá gốc." });
                }

                product.IsSale = true;
                product.SalePrice = salePrice;
            }
            else
            {
                // Tắt Sale: reset giá sale về 0
                product.IsSale = false;
                product.SalePrice = 0;
            }

            _context.SaveChanges();

            // Tính phần trăm giảm giá
            var discountPercent = product.IsSale && product.Price > 0
                ? (int)Math.Round((1 - product.SalePrice / product.Price) * 100)
                : 0;

            return Json(new
            {
                success = true,
                isSale = product.IsSale,
                salePrice = product.SalePrice,
                discountPercent = discountPercent
            });
        }

        // Action AJAX: Bật/tắt trạng thái BestSelling cho sản phẩm.
        [HttpPost]
        public IActionResult ToggleBestSelling(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            product.IsBestSelling = !product.IsBestSelling;
            _context.SaveChanges();

            return Json(new { success = true, isBestSelling = product.IsBestSelling });
        }

        // Action AJAX: Cập nhật thứ tự hiển thị riêng biệt cho sản phẩm.
        [HttpPost]
        public IActionResult ToggleDisplayOrder(int id, string type, int displayOrder)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            if (displayOrder < 0) displayOrder = 0;

            if (type == "new")
            {
                product.DisplayOrderNew = displayOrder;
            }
            else if (type == "sale")
            {
                product.DisplayOrderSale = displayOrder;
            }
            else if (type == "best")
            {
                product.DisplayOrderBestSelling = displayOrder;
            }
            else
            {
                return Json(new { success = false, message = "Loại trạng thái không hợp lệ." });
            }

            _context.SaveChanges();

            return Json(new { success = true, type = type, displayOrder = displayOrder });
        }

        // Action POST: Xóa nhiều sản phẩm đã chọn.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSelected(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sản phẩm để xóa.";
                return RedirectToAction("Index");
            }

            var products = _context.Products.Where(p => ids.Contains(p.Id)).ToList();
            _context.Products.RemoveRange(products);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Đã xóa thành công {products.Count} sản phẩm.";
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
