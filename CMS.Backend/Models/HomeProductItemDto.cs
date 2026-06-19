namespace CMS.Backend.Models
{
    public class HomeProductItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryProductId { get; set; }
        public bool IsNew { get; set; }
        public bool IsSale { get; set; }
        public decimal SalePrice { get; set; }
        public bool IsBestSelling { get; set; }
        public int DiscountPercent { get; set; }
        public int SoldQuantity { get; set; }
    }
}
