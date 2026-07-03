using CMS.Backend.Services;

namespace CMS.Backend.Models
{
    public class AdminProductReviewIndexViewModel
    {
        public AdminReviewFilter Filter { get; set; } = new();
        public PagedResult<AdminProductReviewDto> Reviews { get; set; } = new();
        public AdminProductReviewStatsDto Stats { get; set; } = new();
    }

    public class AdminProductReviewDetailViewModel
    {
        public AdminProductReviewDetailDto Review { get; set; } = new();
        public string? ReplyContent { get; set; }
        public string? ModerationReason { get; set; }
    }
}
