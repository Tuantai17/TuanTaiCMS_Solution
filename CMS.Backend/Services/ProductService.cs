using CMS.Backend.Models;
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasOrderHistoryAsync(int productId)
        {
            return await _context.OrderDetails
                .AsNoTracking()
                .AnyAsync(x => x.ProductId == productId);
        }

        public async Task<ProductActionResult> SoftDeleteAsync(int productId, string deletedBy, string? reason)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return new ProductActionResult { Success = false, Message = "Không tìm thấy sản phẩm." };
            }

            var hasOrder = await HasOrderHistoryAsync(productId);
            if (hasOrder)
            {
                return new ProductActionResult 
                { 
                    Success = false, 
                    Message = "Không thể xóa sản phẩm vì sản phẩm đã phát sinh đơn hàng. Bạn có thể chuyển sản phẩm sang trạng thái Ngừng kinh doanh.",
                    HasOrderHistory = true
                };
            }

            product.IsDeleted = true;
            product.DeletedAt = DateTime.Now;
            product.DeletedBy = deletedBy;
            product.DeleteReason = reason?.Length > 500 ? reason.Substring(0, 500) : reason;

            await _context.SaveChangesAsync();

            return new ProductActionResult { Success = true, Message = "Đã chuyển sản phẩm vào thùng rác." };
        }

        public async Task<BulkProductActionResult> BulkSoftDeleteAsync(IEnumerable<int> productIds, string deletedBy, string? reason)
        {
            var result = new BulkProductActionResult();
            var distinctIds = productIds.Distinct().ToList();

            if (!distinctIds.Any()) return result;

            var products = await _context.Products
                .Where(p => distinctIds.Contains(p.Id))
                .ToListAsync();

            foreach (var product in products)
            {
                if (await HasOrderHistoryAsync(product.Id))
                {
                    result.FailedCount++;
                    result.FailedProducts.Add(product.Name);
                }
                else
                {
                    product.IsDeleted = true;
                    product.DeletedAt = DateTime.Now;
                    product.DeletedBy = deletedBy;
                    product.DeleteReason = reason?.Length > 500 ? reason.Substring(0, 500) : reason;
                    result.SuccessCount++;
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<ProductActionResult> RestoreAsync(int productId)
        {
            var product = await _context.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsDeleted);

            if (product == null)
            {
                return new ProductActionResult { Success = false, Message = "Không tìm thấy sản phẩm trong thùng rác." };
            }

            product.IsDeleted = false;
            product.DeletedAt = null;
            product.DeletedBy = null;
            product.DeleteReason = null;

            await _context.SaveChangesAsync();

            return new ProductActionResult { Success = true, Message = "Khôi phục sản phẩm thành công." };
        }

        public async Task<BulkProductActionResult> BulkRestoreAsync(IEnumerable<int> productIds)
        {
            var result = new BulkProductActionResult();
            var distinctIds = productIds.Distinct().ToList();

            if (!distinctIds.Any()) return result;

            var products = await _context.Products
                .IgnoreQueryFilters()
                .Where(p => distinctIds.Contains(p.Id) && p.IsDeleted)
                .ToListAsync();

            foreach (var product in products)
            {
                product.IsDeleted = false;
                product.DeletedAt = null;
                product.DeletedBy = null;
                product.DeleteReason = null;
                result.SuccessCount++;
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<ProductActionResult> PermanentDeleteAsync(int productId)
        {
            var product = await _context.Products
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsDeleted);

            if (product == null)
            {
                return new ProductActionResult { Success = false, Message = "Không tìm thấy sản phẩm trong thùng rác." };
            }

            var hasOrder = await HasOrderHistoryAsync(productId);
            if (hasOrder)
            {
                return new ProductActionResult 
                { 
                    Success = false, 
                    Message = "Không thể xóa vĩnh viễn vì sản phẩm đã phát sinh đơn hàng.",
                    HasOrderHistory = true
                };
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return new ProductActionResult { Success = true, Message = "Đã xóa vĩnh viễn sản phẩm." };
        }

        public async Task<BulkProductActionResult> BulkPermanentDeleteAsync(IEnumerable<int> productIds)
        {
            var result = new BulkProductActionResult();
            var distinctIds = productIds.Distinct().ToList();

            if (!distinctIds.Any()) return result;

            var products = await _context.Products
                .IgnoreQueryFilters()
                .Where(p => distinctIds.Contains(p.Id) && p.IsDeleted)
                .ToListAsync();

            var productsToRemove = new List<Product>();

            foreach (var product in products)
            {
                if (await HasOrderHistoryAsync(product.Id))
                {
                    result.FailedCount++;
                    result.FailedProducts.Add(product.Name);
                }
                else
                {
                    productsToRemove.Add(product);
                    result.SuccessCount++;
                }
            }

            if (productsToRemove.Any())
            {
                _context.Products.RemoveRange(productsToRemove);
                await _context.SaveChangesAsync();
            }

            return result;
        }

        public async Task<ProductTrashViewModel> GetTrashAsync(ProductTrashViewModel filter)
        {
            var query = _context.Products
                .IgnoreQueryFilters()
                .Where(p => p.IsDeleted)
                .Include(p => p.CategoryProduct)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keywordLower = filter.Keyword.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(keywordLower) || p.Id.ToString() == keywordLower);
            }

            if (filter.CategoryId.HasValue && filter.CategoryId > 0)
            {
                query = query.Where(p => p.CategoryProductId == filter.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.DeletedTime))
            {
                var today = DateTime.Today;
                switch (filter.DeletedTime.ToLower())
                {
                    case "today":
                        query = query.Where(p => p.DeletedAt >= today);
                        break;
                    case "7days":
                        query = query.Where(p => p.DeletedAt >= today.AddDays(-7));
                        break;
                    case "30days":
                        query = query.Where(p => p.DeletedAt >= today.AddDays(-30));
                        break;
                }
            }

            filter.TotalItems = await query.CountAsync();
            filter.TotalPages = (int)Math.Ceiling(filter.TotalItems / (double)filter.PageSize);
            
            if (filter.Page > filter.TotalPages && filter.TotalPages > 0) filter.Page = filter.TotalPages;
            if (filter.Page < 1) filter.Page = 1;

            if (filter.SortBy == "oldest")
            {
                query = query.OrderBy(p => p.DeletedAt);
            }
            else if (filter.SortBy == "name_asc")
            {
                query = query.OrderBy(p => p.Name);
            }
            else if (filter.SortBy == "name_desc")
            {
                query = query.OrderByDescending(p => p.Name);
            }
            else
            {
                query = query.OrderByDescending(p => p.DeletedAt);
            }

            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            filter.Items = products.Select(p => new ProductTrashItemViewModel
            {
                ProductId = p.Id,
                ProductCode = $"#SP{p.Id:D3}",
                SKU = "",
                ProductName = p.Name,
                ImageUrl = p.ImageUrl,
                CategoryName = p.CategoryProduct?.Name ?? "Chưa phân loại",
                Price = p.Price,
                SalePrice = p.IsSale ? p.SalePrice : null,
                StockQuantity = p.StockQuantity,
                DeletedBy = p.DeletedBy ?? "N/A",
                DeletedAt = p.DeletedAt,
                DeleteReason = p.DeleteReason,
                HasOrderHistory = false, // Check performed at service/view if needed
                CanPermanentDelete = true
            }).ToList();

            // Populate HasOrderHistory per item
            foreach (var item in filter.Items)
            {
                item.HasOrderHistory = await HasOrderHistoryAsync(item.ProductId);
                item.CanPermanentDelete = !item.HasOrderHistory;
            }

            return filter;
        }

        public async Task<int> GetTrashCountAsync()
        {
            return await _context.Products
                .IgnoreQueryFilters()
                .CountAsync(p => p.IsDeleted);
        }
    }
}
