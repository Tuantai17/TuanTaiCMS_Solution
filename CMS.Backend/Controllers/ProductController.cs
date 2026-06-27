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
using CMS.Backend.Services;
using CMS.Backend.Models;

namespace CMS.Backend.Controllers
{
    // ProductController xử lý các request bắt đầu bằng /Product.
    [Authorize(Roles = "Admin,Staff")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;

        public ProductController(ApplicationDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        // Action Index hiển thị danh sách sản phẩm có phân trang và bộ lọc danh mục.
        // categoryId: mã danh mục cần lọc (mặc định = null là không lọc)
        // page: trang hiện tại (mặc định = 1)
        // pageSize: số sản phẩm mỗi trang (mặc định = 10)
        public IActionResult Index(int? categoryId = null, string? stockStatus = null, int page = 1, int pageSize = 10)
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

            // Lọc sản phẩm theo tình trạng kho
            if (!string.IsNullOrEmpty(stockStatus))
            {
                if (stockStatus == "low")
                {
                    query = query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= 10);
                }
                else if (stockStatus == "out")
                {
                    query = query.Where(p => p.StockQuantity <= 0);
                }
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
            ViewBag.StockStatus = stockStatus;

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
            return View(new Product());
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

        // Action SoftDelete xóa tạm sản phẩm theo id.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(int id, string? reason)
        {
            var username = User.Identity?.Name ?? "Unknown Admin";
            var result = await _productService.SoftDeleteAsync(id, username, reason);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
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

        // Action POST: Chuyển nhiều sản phẩm đã chọn vào thùng rác.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkSoftDelete(List<int> ids, string? reason)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sản phẩm để thao tác.";
                return RedirectToAction("Index");
            }

            var username = User.Identity?.Name ?? "Unknown Admin";
            var result = await _productService.BulkSoftDeleteAsync(ids, username, reason);

            if (result.SuccessCount > 0 && result.FailedCount == 0)
            {
                TempData["SuccessMessage"] = $"Đã chuyển thành công {result.SuccessCount} sản phẩm vào thùng rác.";
            }
            else if (result.SuccessCount > 0 && result.FailedCount > 0)
            {
                TempData["SuccessMessage"] = $"Đã chuyển {result.SuccessCount} sản phẩm vào thùng rác. Có {result.FailedCount} sản phẩm được giữ lại do đã phát sinh đơn hàng.";
            }
            else if (result.SuccessCount == 0 && result.FailedCount > 0)
            {
                TempData["ErrorMessage"] = $"Không thể xóa bất kỳ sản phẩm nào vì tất cả đã phát sinh đơn hàng.";
            }

            return RedirectToAction("Index");
        }

        // Action GET Trash
        public async Task<IActionResult> Trash([FromQuery] ProductTrashViewModel filter)
        {
            var result = await _productService.GetTrashAsync(filter);
            LoadCategoryProductList(filter.CategoryId);
            return View(result);
        }

        // Action POST Restore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _productService.RestoreAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            return RedirectToAction("Trash");
        }

        // Action POST Bulk Restore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkRestore(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sản phẩm để thao tác.";
                return RedirectToAction("Trash");
            }

            var result = await _productService.BulkRestoreAsync(ids);
            if (result.SuccessCount > 0)
            {
                TempData["SuccessMessage"] = $"Đã khôi phục thành công {result.SuccessCount} sản phẩm.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể khôi phục các sản phẩm đã chọn.";
            }

            return RedirectToAction("Trash");
        }

        // Action POST Permanent Delete
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            var result = await _productService.PermanentDeleteAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            return RedirectToAction("Trash");
        }

        // Action POST Bulk Permanent Delete
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkPermanentDelete(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sản phẩm để thao tác.";
                return RedirectToAction("Trash");
            }

            var result = await _productService.BulkPermanentDeleteAsync(ids);
            if (result.SuccessCount > 0 && result.FailedCount == 0)
            {
                TempData["SuccessMessage"] = $"Đã xóa vĩnh viễn thành công {result.SuccessCount} sản phẩm.";
            }
            else if (result.SuccessCount > 0 && result.FailedCount > 0)
            {
                TempData["SuccessMessage"] = $"Đã xóa vĩnh viễn {result.SuccessCount} sản phẩm. Có {result.FailedCount} sản phẩm không thể xóa vì đã phát sinh đơn hàng.";
            }
            else if (result.SuccessCount == 0 && result.FailedCount > 0)
            {
                TempData["ErrorMessage"] = $"Không thể xóa vĩnh viễn các sản phẩm này do đã phát sinh đơn hàng.";
            }

            return RedirectToAction("Trash");
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
