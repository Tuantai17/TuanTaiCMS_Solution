using CMS.Data.Enums;
using Microsoft.AspNetCore.Http;

namespace CMS.Backend.Services
{
    public interface IProductReviewService
    {
        Task<ReviewEligibilityResult> CheckEligibilityAsync(int orderDetailId, int customerId);
        Task<ProductReviewDto> CreateReviewAsync(CreateProductReviewRequest request, int customerId);
        Task<PagedResult<ProductReviewDto>> GetProductReviewsAsync(int productId, ProductReviewFilter filter);
        Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(int productId);
        Task<PagedResult<MyProductReviewDto>> GetMyReviewsAsync(int customerId, MyReviewFilter filter);
        Task<PagedResult<AdminProductReviewDto>> GetAdminReviewsAsync(AdminReviewFilter filter);
        Task<AdminProductReviewDetailDto?> GetAdminReviewDetailAsync(int reviewId);
        Task PublishReviewAsync(int reviewId, int adminUserId);
        Task HideReviewAsync(int reviewId, int adminUserId, string reason);
        Task RejectReviewAsync(int reviewId, int adminUserId, string reason);
        Task<ProductReviewReplyDto> ReplyToReviewAsync(int reviewId, string content, int adminUserId);
        Task<ProductReviewDto?> GetReviewByIdAsync(int reviewId, int customerId);
    }

    public class CreateProductReviewRequest
    {
        public int OrderDetailId { get; set; }
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public List<IFormFile>? Images { get; set; }
    }

    public class ReviewEligibilityResult
    {
        public bool CanReview { get; set; }
        public bool AlreadyReviewed { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ExistingReviewId { get; set; }
    }

    public class ProductReviewFilter
    {
        public int? Rating { get; set; }
        public bool? HasImages { get; set; }
        public string SortBy { get; set; } = "newest";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;
    }

    public class MyReviewFilter
    {
        public ReviewStatus? Status { get; set; }
        public bool? HasReply { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminReviewFilter
    {
        public string? Keyword { get; set; }
        public int? ProductId { get; set; }
        public int? Rating { get; set; }
        public ReviewStatus? Status { get; set; }
        public bool? HasReply { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class ProductReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public int OrderDetailId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; }
        public bool IsEdited { get; set; }
        public ReviewStatus Status { get; set; }
        public string? ModerationReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ProductReviewImageDto> Images { get; set; } = new();
        public List<ProductReviewReplyDto> Replies { get; set; } = new();
    }

    public class ProductReviewImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class ProductReviewReplyDto
    {
        public int Id { get; set; }
        public int AdminUserId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsOfficial { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ProductReviewSummaryDto
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
    }

    public class MyProductReviewDto : ProductReviewDto
    {
        public string? ReviewStatusLabel { get; set; }
    }

    public class AdminProductReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerAvatar { get; set; }
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public ReviewStatus Status { get; set; }
        public int ImageCount { get; set; }
        public bool HasReply { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminProductReviewDetailDto : ProductReviewDto
    {
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public int CustomerId { get; set; }
        public int AdminReplyCount { get; set; }
    }

    public class AdminProductReviewStatsDto
    {
        public int TotalReviews { get; set; }
        public int PendingReviews { get; set; }
        public int PublishedReviews { get; set; }
        public int HiddenReviews { get; set; }
        public int UnrepliedReviews { get; set; }
    }
}
