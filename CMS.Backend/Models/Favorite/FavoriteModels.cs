using System;
using System.Collections.Generic;

namespace CMS.Backend.Models.Favorite
{
    public class FavoriteProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string ImageUrl { get; set; }
        public string BrandName { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public int DiscountPercent { get; set; }
        public int StockQuantity { get; set; }
        public bool IsOutOfStock { get; set; }
        public bool IsNew { get; set; }
        public bool IsBestSelling { get; set; }
        public bool IsSale { get; set; }
        public bool IsFavorite { get; set; }
        public DateTime FavoritedAt { get; set; }
    }

    public class FavoriteListResponse
    {
        public List<FavoriteProductDto> Items { get; set; } = new List<FavoriteProductDto>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
