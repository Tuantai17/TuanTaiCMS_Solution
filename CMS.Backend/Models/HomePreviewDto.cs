namespace CMS.Backend.Models
{
    public class HomePreviewDto
    {
        public List<HomeProductItemDto> BestSellingProducts { get; set; } = new();
        public List<HomeProductItemDto> NewestProducts { get; set; } = new();
        public List<HomeProductItemDto> SaleProducts { get; set; } = new();
        public List<HomePostItemDto> FeaturedPosts { get; set; } = new();
    }
}
