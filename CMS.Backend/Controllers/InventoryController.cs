using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Inventory
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? keyword = null, int? categoryId = null, string? stockStatus = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Inventories
                .Include(i => i.Product)
                .ThenInclude(p => p.CategoryProduct)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(i => i.Product.Name.ToLower().Contains(keyword) || i.Product.Id.ToString() == keyword);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(i => i.Product.CategoryProductId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(stockStatus))
            {
                if (stockStatus == "low")
                {
                    query = query.Where(i => (i.CurrentStock - i.ReservedStock) > 0 && (i.CurrentStock - i.ReservedStock) <= i.AlertThreshold);
                }
                else if (stockStatus == "out")
                {
                    query = query.Where(i => (i.CurrentStock - i.ReservedStock) <= 0);
                }
                else if (stockStatus == "in")
                {
                    query = query.Where(i => (i.CurrentStock - i.ReservedStock) > i.AlertThreshold);
                }
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(i => i.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Statistics
            var allInventories = await _context.Inventories.ToListAsync();
            int totalSku = allInventories.Count(i => i.IsActive);
            int inStock = allInventories.Count(i => i.AvailableStock > i.AlertThreshold);
            int lowStock = allInventories.Count(i => i.AvailableStock > 0 && i.AvailableStock <= i.AlertThreshold);
            int outOfStock = allInventories.Count(i => i.AvailableStock <= 0);
            decimal totalValue = allInventories.Sum(i => i.CurrentStock * i.CostPrice);

            ViewBag.TotalSku = totalSku;
            ViewBag.InStock = inStock;
            ViewBag.LowStock = lowStock;
            ViewBag.OutOfStock = outOfStock;
            ViewBag.TotalValue = totalValue;

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.StockStatus = stockStatus;

            ViewBag.CategoryList = new SelectList(await _context.CategoriesProducts.ToListAsync(), "Id", "Name", categoryId);

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Import(int[] inventoryIds, int quantity, decimal unitCost, string note)
        {
            if (inventoryIds == null || !inventoryIds.Any()) return BadRequest("Vui lòng chọn sản phẩm");
            if (quantity <= 0) return BadRequest("Số lượng phải lớn hơn 0");

            var inventories = await _context.Inventories.Where(i => inventoryIds.Contains(i.Id)).ToListAsync();
            
            foreach(var inv in inventories) 
            {
                int stockBefore = inv.CurrentStock;
                inv.CurrentStock += quantity;
                inv.CostPrice = unitCost; 
                inv.UpdatedAt = DateTime.Now;

                var trans = new InventoryTransaction
                {
                    InventoryId = inv.Id,
                    TransactionType = "IMPORT",
                    QuantityChange = quantity,
                    StockBefore = stockBefore,
                    StockAfter = inv.CurrentStock,
                    ReservedBefore = inv.ReservedStock,
                    ReservedAfter = inv.ReservedStock,
                    UnitCost = unitCost,
                    Note = note,
                    CreatedBy = User.Identity?.Name ?? "System",
                    CreatedAt = DateTime.Now
                };

                var product = await _context.Products.FindAsync(inv.ProductId);
                if(product != null) product.StockQuantity = inv.CurrentStock;

                _context.InventoryTransactions.Add(trans);
            }
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Nhập kho thành công cho {inventories.Count} sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Export(int[] inventoryIds, int quantity, string note)
        {
            if (inventoryIds == null || !inventoryIds.Any()) return BadRequest("Vui lòng chọn sản phẩm");
            if (quantity <= 0) return BadRequest("Số lượng phải lớn hơn 0");

            var inventories = await _context.Inventories.Where(i => inventoryIds.Contains(i.Id)).ToListAsync();
            int successCount = 0;

            foreach(var inv in inventories)
            {
                if (inv.AvailableStock >= quantity)
                {
                    int stockBefore = inv.CurrentStock;
                    inv.CurrentStock -= quantity;
                    inv.UpdatedAt = DateTime.Now;

                    var trans = new InventoryTransaction
                    {
                        InventoryId = inv.Id,
                        TransactionType = "EXPORT",
                        QuantityChange = -quantity,
                        StockBefore = stockBefore,
                        StockAfter = inv.CurrentStock,
                        ReservedBefore = inv.ReservedStock,
                        ReservedAfter = inv.ReservedStock,
                        Note = note,
                        CreatedBy = User.Identity?.Name ?? "System",
                        CreatedAt = DateTime.Now
                    };

                    var product = await _context.Products.FindAsync(inv.ProductId);
                    if(product != null) product.StockQuantity = inv.CurrentStock;

                    _context.InventoryTransactions.Add(trans);
                    successCount++;
                }
            }
            
            await _context.SaveChangesAsync();

            if (successCount == 0) {
                TempData["ErrorMessage"] = "Không đủ số lượng có thể xuất cho các sản phẩm đã chọn.";
            } else {
                TempData["SuccessMessage"] = $"Xuất kho thành công cho {successCount} sản phẩm.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Adjust(int inventoryId, int newCurrentStock, int newAlertThreshold, string reason)
        {
            if (newCurrentStock < 0) return BadRequest("Tồn kho không được âm");

            var inv = await _context.Inventories.FindAsync(inventoryId);
            if (inv == null) return NotFound();

            int stockBefore = inv.CurrentStock;
            int change = newCurrentStock - stockBefore;
            
            inv.CurrentStock = newCurrentStock;
            inv.AlertThreshold = newAlertThreshold;
            inv.UpdatedAt = DateTime.Now;

            var trans = new InventoryTransaction
            {
                InventoryId = inv.Id,
                TransactionType = "ADJUSTMENT",
                QuantityChange = change,
                StockBefore = stockBefore,
                StockAfter = inv.CurrentStock,
                ReservedBefore = inv.ReservedStock,
                ReservedAfter = inv.ReservedStock,
                Reason = reason,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedAt = DateTime.Now
            };

            // Update Product stock for backward compatibility
            var product = await _context.Products.FindAsync(inv.ProductId);
            if(product != null) product.StockQuantity = inv.CurrentStock;

            _context.InventoryTransactions.Add(trans);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Điều chỉnh kho thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> History(int page = 1, int pageSize = 15)
        {
            var query = _context.InventoryTransactions
                .Include(t => t.Inventory)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.CategoryProduct)
                .OrderByDescending(t => t.CreatedAt);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(items);
        }
    }
}
