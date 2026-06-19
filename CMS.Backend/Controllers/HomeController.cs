using CMS.Backend.Models;
using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    [Route("api/Home")]
    [ApiController]
    public class HomeApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("products/best-selling")]
        [ProducesResponseType(typeof(List<HomeProductItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBestSellingProducts([FromQuery] int? take = null)
        {
            IQueryable<HomeProductItemDto> query = BuildProductQuery()
                .Where(p => p.IsBestSelling || p.SoldQuantity > 0)
                .OrderByDescending(p => p.IsBestSelling)
                .ThenByDescending(p => p.SoldQuantity)
                .ThenByDescending(p => p.Id);

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(Math.Min(take.Value, 50));
            }

            var products = await query.ToListAsync();

            return Ok(products);
        }

        [HttpGet("products/newest")]
        [ProducesResponseType(typeof(List<HomeProductItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNewestProducts([FromQuery] int? take = null)
        {
            IQueryable<HomeProductItemDto> query = BuildProductQuery()
                .Where(p => p.IsNew)
                .OrderByDescending(p => p.Id);

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(Math.Min(take.Value, 50));
            }

            var products = await query.ToListAsync();

            return Ok(products);
        }

        [HttpGet("products/sale")]
        [ProducesResponseType(typeof(List<HomeProductItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSaleProducts([FromQuery] int? take = null)
        {
            IQueryable<HomeProductItemDto> query = BuildProductQuery()
                .Where(p => p.IsSale)
                .OrderByDescending(p => p.Id);

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(Math.Min(take.Value, 50));
            }

            var products = await query.ToListAsync();

            return Ok(products);
        }

        [HttpGet("posts/featured")]
        [ProducesResponseType(typeof(List<HomePostItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeaturedPosts([FromQuery] int? take = null)
        {
            IQueryable<HomePostItemDto> query = BuildFeaturedPostQuery();

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(Math.Min(take.Value, 50));
            }

            var posts = await query.ToListAsync();

            return Ok(posts);
        }

        [HttpGet("preview")]
        [ProducesResponseType(typeof(HomePreviewDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHomePreview([FromQuery] int? productTake = null, [FromQuery] int? postTake = null)
        {
            var hasProductTake = productTake.HasValue && productTake.Value > 0;
            var hasPostTake = postTake.HasValue && postTake.Value > 0;
            var normalizedProductTake = hasProductTake ? Math.Min(productTake!.Value, 50) : 0;
            var normalizedPostTake = hasPostTake ? Math.Min(postTake!.Value, 50) : 0;

            IQueryable<HomeProductItemDto> bestSellingQuery = BuildProductQuery()
                .Where(p => p.IsBestSelling || p.SoldQuantity > 0)
                .OrderByDescending(p => p.IsBestSelling)
                .ThenByDescending(p => p.SoldQuantity)
                .ThenByDescending(p => p.Id);

            IQueryable<HomeProductItemDto> newestQuery = BuildProductQuery()
                .Where(p => p.IsNew)
                .OrderByDescending(p => p.Id);

            IQueryable<HomeProductItemDto> saleQuery = BuildProductQuery()
                .Where(p => p.IsSale)
                .OrderByDescending(p => p.Id);

            IQueryable<HomePostItemDto> featuredPostsQuery = BuildFeaturedPostQuery();

            if (hasProductTake)
            {
                bestSellingQuery = bestSellingQuery.Take(normalizedProductTake);
                newestQuery = newestQuery.Take(normalizedProductTake);
                saleQuery = saleQuery.Take(normalizedProductTake);
            }

            if (hasPostTake)
            {
                featuredPostsQuery = featuredPostsQuery.Take(normalizedPostTake);
            }

            var preview = new HomePreviewDto
            {
                BestSellingProducts = await bestSellingQuery.ToListAsync(),
                NewestProducts = await newestQuery.ToListAsync(),
                SaleProducts = await saleQuery.ToListAsync(),
                FeaturedPosts = await featuredPostsQuery.ToListAsync()
            };

            return Ok(preview);
        }

        private IQueryable<HomeProductItemDto> BuildProductQuery()
        {
            return _context.Products
                .AsNoTracking()
                .Select(p => new HomeProductItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    StockQuantity = p.StockQuantity,
                    CategoryProductId = p.CategoryProductId,
                    IsNew = p.IsNew,
                    IsSale = p.IsSale,
                    SalePrice = p.SalePrice,
                    IsBestSelling = p.IsBestSelling,
                    DiscountPercent = p.IsSale && p.Price > 0
                        ? (int)Math.Round((1 - p.SalePrice / p.Price) * 100)
                        : 0,
                    SoldQuantity = _context.OrderDetails
                        .Where(od => od.ProductId == p.Id)
                        .Sum(od => (int?)od.Quantity) ?? 0
                });
        }

        private IQueryable<HomePostItemDto> BuildFeaturedPostQuery()
        {
            return _context.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.IsFeatured)
                .OrderByDescending(p => p.Id)
                .Select(p => new HomePostItemDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    ImageUrl = p.ImageUrl,
                    CreatedDate = p.CreatedDate,
                    CategoryId = p.CategoryId,
                    IsFeatured = p.IsFeatured,
                    ShortDescription = string.IsNullOrWhiteSpace(p.Content)
                        ? "Dang cap nhat noi dung tom tat cho bai viet..."
                        : (p.Content.Length > 180 ? p.Content.Substring(0, 180) + "..." : p.Content),
                    CategoryName = p.Category != null ? p.Category.Name : "Khong xac dinh"
                });
        }

    }
}
