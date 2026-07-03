using CMS.Backend.Helpers;
using CMS.Backend.Services;
using CMS.Data.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Backend.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    public class ProductReviewsController : ControllerBase
    {
        private readonly IProductReviewService _productReviewService;
        private readonly IConfiguration _configuration;

        public ProductReviewsController(
            IProductReviewService productReviewService,
            IConfiguration configuration)
        {
            _productReviewService = productReviewService;
            _configuration = configuration;
        }

        [HttpGet("eligibility/{orderDetailId:int}")]
        public async Task<IActionResult> CheckEligibility(int orderDetailId)
        {
            if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
            {
                return authError!;
            }

            var result = await _productReviewService.CheckEligibilityAsync(orderDetailId, customerId);
            return Ok(result);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateReview([FromForm] CreateProductReviewRequest request)
        {
            if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
            {
                return authError!;
            }

            try
            {
                var review = await _productReviewService.CreateReviewAsync(request, customerId);
                return Ok(new { message = "Gui danh gia thanh cong. Danh gia dang cho duyet.", review });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetProductReviews(
            int productId,
            [FromQuery] int? rating,
            [FromQuery] bool? hasImages,
            [FromQuery] string? sortBy,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            var result = await _productReviewService.GetProductReviewsAsync(productId, new ProductReviewFilter
            {
                Rating = rating,
                HasImages = hasImages,
                SortBy = string.IsNullOrWhiteSpace(sortBy) ? "newest" : sortBy,
                Page = page,
                PageSize = pageSize
            });

            return Ok(result);
        }

        [HttpGet("product/{productId:int}/summary")]
        public async Task<IActionResult> GetProductReviewSummary(int productId)
        {
            var result = await _productReviewService.GetProductReviewSummaryAsync(productId);
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyReviews(
            [FromQuery] ReviewStatus? status,
            [FromQuery] bool? hasReply,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
            {
                return authError!;
            }

            var result = await _productReviewService.GetMyReviewsAsync(customerId, new MyReviewFilter
            {
                Status = status,
                HasReply = hasReply,
                Page = page,
                PageSize = pageSize
            });

            return Ok(result);
        }

        [HttpGet("my/{reviewId:int}")]
        public async Task<IActionResult> GetMyReviewById(int reviewId)
        {
            if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
            {
                return authError!;
            }

            var review = await _productReviewService.GetReviewByIdAsync(reviewId, customerId);
            if (review == null)
            {
                return NotFound(new { message = "Khong tim thay danh gia." });
            }

            return Ok(review);
        }

        private bool TryGetAuthenticatedCustomerId(out int customerId, out IActionResult? errorResult)
        {
            customerId = 0;
            errorResult = null;

            var authorizationHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authorizationHeader) ||
                !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                errorResult = Unauthorized(new { message = "Phien dang nhap da het han. Vui long dang nhap lai." });
                return false;
            }

            var token = authorizationHeader["Bearer ".Length..].Trim();
            var secret = _configuration["CustomerSession:Secret"] ?? "TuanTaiCMS.CustomerSession.Secret.2026";
            if (!CustomerSessionTokenHelper.TryValidateToken(token, secret, out customerId))
            {
                errorResult = Unauthorized(new { message = "Phien dang nhap da het han. Vui long dang nhap lai." });
                return false;
            }

            return true;
        }
    }
}
