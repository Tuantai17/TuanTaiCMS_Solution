using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Models.Favorite;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Services.Favorite
{
    public class ProductFavoriteService : IProductFavoriteService
    {
        private readonly ApplicationDbContext _context;

        public ProductFavoriteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddFavoriteAsync(int customerId, int productId)
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists) return false;

            var existing = await _context.ProductFavorites
                .FirstOrDefaultAsync(pf => pf.CustomerId == customerId && pf.ProductId == productId);
                
            if (existing != null) return true; // Already favorite

            var fav = new ProductFavorite
            {
                CustomerId = customerId,
                ProductId = productId,
                CreatedAt = DateTime.Now
            };

            _context.ProductFavorites.Add(fav);
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                // In case of concurrent inserts violating unique constraint
                return true;
            }
        }

        public async Task<bool> RemoveFavoriteAsync(int customerId, int productId)
        {
            var fav = await _context.ProductFavorites
                .FirstOrDefaultAsync(pf => pf.CustomerId == customerId && pf.ProductId == productId);
                
            if (fav == null) return false;

            _context.ProductFavorites.Remove(fav);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFavoriteAsync(int customerId, int productId)
        {
            return await _context.ProductFavorites
                .AnyAsync(pf => pf.CustomerId == customerId && pf.ProductId == productId);
        }

        public async Task<FavoriteListResponse> GetFavoritesAsync(int customerId, int page, int pageSize, string keyword = null)
        {
            var query = _context.ProductFavorites
                .Include(pf => pf.Product)
                .ThenInclude(p => p.CategoryProduct)
                .Where(pf => pf.CustomerId == customerId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(pf => pf.Product.Name.Contains(keyword)); // SKU was not in Product.cs but Name is. Let's stick to Name.
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(pf => pf.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(pf => new FavoriteProductDto
                {
                    Id = pf.ProductId,
                    Name = pf.Product.Name,
                    SKU = "SKU" + (120000 + pf.Product.Id), // Fallback SKU as seen in ProductCard
                    ImageUrl = pf.Product.ImageUrl,
                    BrandName = pf.Product.CategoryProduct != null ? pf.Product.CategoryProduct.Name : "Khác",
                    Price = pf.Product.Price,
                    SalePrice = pf.Product.IsSale ? pf.Product.SalePrice : null,
                    DiscountPercent = pf.Product.IsSale && pf.Product.Price > 0 ? (int)Math.Round((1 - pf.Product.SalePrice / pf.Product.Price) * 100) : 0,
                    StockQuantity = pf.Product.StockQuantity,
                    IsOutOfStock = pf.Product.StockQuantity <= 0,
                    IsNew = pf.Product.IsNew,
                    IsBestSelling = pf.Product.IsBestSelling,
                    IsSale = pf.Product.IsSale,
                    IsFavorite = true,
                    FavoritedAt = pf.CreatedAt
                })
                .ToListAsync();

            return new FavoriteListResponse
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        public async Task<HashSet<int>> GetFavoriteProductIdsAsync(int customerId, IEnumerable<int> productIds)
        {
            var favIds = await _context.ProductFavorites
                .Where(pf => pf.CustomerId == customerId && productIds.Contains(pf.ProductId))
                .Select(pf => pf.ProductId)
                .ToListAsync();

            return new HashSet<int>(favIds);
        }

        public async Task<int> GetFavoriteCountAsync(int customerId)
        {
            return await _context.ProductFavorites.CountAsync(pf => pf.CustomerId == customerId);
        }
    }
}
