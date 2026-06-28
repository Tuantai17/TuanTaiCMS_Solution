using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ProductsController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? filter = null,
        [FromQuery] int? skip = null,
        [FromQuery] int? take = null)
    {
      try
      {
        var query = _context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
          var searchLower = search.Trim().ToLower();
          query = query.Where(p => p.Name.ToLower().Contains(searchLower));
        }

        if (minPrice.HasValue)
        {
          query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
          query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
          var categoryIds = await _context.CategoriesProducts
              .Where(c => c.Id == categoryId.Value || c.ParentId == categoryId.Value)
              .Select(c => c.Id)
              .ToListAsync();

          query = query.Where(p => categoryIds.Contains(p.CategoryProductId));
        }

        // Loc theo trang thai New hoac Sale
        if (!string.IsNullOrWhiteSpace(filter))
        {
          if (filter.Equals("new", StringComparison.OrdinalIgnoreCase))
            query = query.Where(p => p.IsNew);
          else if (filter.Equals("sale", StringComparison.OrdinalIgnoreCase))
            query = query.Where(p => p.IsSale);
        }

        var productQuery = query.Select(p => new
        {
          p.Id,
          p.Name,
          p.Price,
          p.ImageUrl,
          p.StockQuantity,
          p.CategoryProductId,
          p.IsNew,
          p.IsSale,
          p.SalePrice,
          p.IsBestSelling,
          p.DisplayOrderNew,
          p.DisplayOrderSale,
          p.DisplayOrderBestSelling,
          DiscountPercent = p.IsSale && p.Price > 0
            ? (int)Math.Round((1 - p.SalePrice / p.Price) * 100)
            : 0,
          SoldQuantity = _context.OrderDetails
            .Where(od => od.ProductId == p.Id)
            .Sum(od => (int?)od.Quantity) ?? 0
        });

        var sortKey = sortBy?.Trim().ToLower();
        var isBestSelling = sortKey == "best-selling" || sortKey == "bestselling" || sortKey == "sold";
        var isNew = filter?.Equals("new", StringComparison.OrdinalIgnoreCase) == true;
        var isSale = filter?.Equals("sale", StringComparison.OrdinalIgnoreCase) == true;

        var filteredProducts = isBestSelling
          ? productQuery.Where(p => p.IsBestSelling || p.SoldQuantity > 0)
          : productQuery;

        IQueryable<dynamic> sortedProducts;
        if (isBestSelling)
        {
          sortedProducts = filteredProducts.OrderBy(p => p.DisplayOrderBestSelling == 0 ? int.MaxValue : p.DisplayOrderBestSelling).ThenByDescending(p => p.IsBestSelling).ThenByDescending(p => p.SoldQuantity).ThenByDescending(p => p.Id);
        }
        else if (isNew)
        {
          sortedProducts = filteredProducts.OrderBy(p => p.DisplayOrderNew == 0 ? int.MaxValue : p.DisplayOrderNew).ThenByDescending(p => p.Id);
        }
        else if (isSale)
        {
          sortedProducts = filteredProducts.OrderBy(p => p.DisplayOrderSale == 0 ? int.MaxValue : p.DisplayOrderSale).ThenByDescending(p => p.Id);
        }
        else
        {
          sortedProducts = filteredProducts.OrderByDescending(p => p.Id); // Mặc định sắp xếp theo ID giảm dần
        }

        var finalQuery = sortedProducts.AsQueryable();
        if (skip.HasValue && skip.Value > 0)
        {
          finalQuery = finalQuery.Skip(skip.Value);
        }
        if (take.HasValue && take.Value > 0)
        {
          finalQuery = finalQuery.Take(take.Value);
        }

        var products = await finalQuery.ToListAsync();

        return Ok(products);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Loi he thong khi tai danh sach san pham", detail = ex.Message });
      }
    }

    [HttpGet("categoryproduct/{categoryProductId}")]
    public async Task<IActionResult> GetByCategoryProduct(int categoryProductId)
    {
      try
      {
        var products = await _context.Products
          .AsNoTracking()
          .Where(p => p.CategoryProductId == categoryProductId)
          .OrderByDescending(p => p.Id)
          .Select(p => new
          {
            p.Id,
            p.Name,
            p.Price,
            p.ImageUrl,
            p.StockQuantity,
            p.CategoryProductId,
            p.IsNew,
            p.IsSale,
            p.SalePrice,
            p.IsBestSelling,
            DiscountPercent = p.IsSale && p.Price > 0
              ? (int)Math.Round((1 - p.SalePrice / p.Price) * 100)
              : 0,
            SoldQuantity = _context.OrderDetails
              .Where(od => od.ProductId == p.Id)
              .Sum(od => (int?)od.Quantity) ?? 0
          })
          .ToListAsync();

        return Ok(products);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Loi he thong khi loc san pham theo danh muc", detail = ex.Message });
      }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(int id)
    {
      try
      {
        var product = await _context.Products
          .AsNoTracking()
          .Include(p => p.CategoryProduct)
          .Include(p => p.ProductImages)
          .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
          return NotFound(new { message = "Khong tim thay san pham nay trong he thong" });
        }

        return Ok(product);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Loi xu ly he thong khi lay chi tiet san pham", detail = ex.Message });
      }
    }
  }
}
