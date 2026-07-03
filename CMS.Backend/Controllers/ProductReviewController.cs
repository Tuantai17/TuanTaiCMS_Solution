using CMS.Backend.Models;
using CMS.Backend.Services;
using CMS.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;

namespace CMS.Backend.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class ProductReviewController : Controller
    {
        private readonly IProductReviewService _productReviewService;
        private readonly ApplicationDbContext _context;

        public ProductReviewController(IProductReviewService productReviewService, ApplicationDbContext context)
        {
            _productReviewService = productReviewService;
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? keyword,
            int? productId,
            int? rating,
            ReviewStatus? status,
            bool? hasReply,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 10)
        {
            var filter = new AdminReviewFilter
            {
                Keyword = keyword,
                ProductId = productId,
                Rating = rating,
                Status = status,
                HasReply = hasReply,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var reviews = await _productReviewService.GetAdminReviewsAsync(filter);
            var stats = new AdminProductReviewStatsDto
            {
                TotalReviews = await _context.ProductReviews.CountAsync(),
                PendingReviews = await _context.ProductReviews.CountAsync(r => r.Status == ReviewStatus.Pending),
                PublishedReviews = await _context.ProductReviews.CountAsync(r => r.Status == ReviewStatus.Published),
                HiddenReviews = await _context.ProductReviews.CountAsync(r => r.Status == ReviewStatus.Hidden),
                UnrepliedReviews = await _context.ProductReviews.CountAsync(r => !r.Replies.Any())
            };

            ViewData["Title"] = "Quản lý đánh giá sản phẩm";

            return View(new AdminProductReviewIndexViewModel
            {
                Filter = filter,
                Reviews = reviews,
                Stats = stats
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var review = await _productReviewService.GetAdminReviewDetailAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Chi tiết đánh giá sản phẩm";
            return View(new AdminProductReviewDetailViewModel
            {
                Review = review
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            try
            {
                await _productReviewService.PublishReviewAsync(id, await GetCurrentAdminUserIdAsync());
                TempData["SuccessMessage"] = "Đã duyệt đánh giá thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id, string reason)
        {
            try
            {
                await _productReviewService.HideReviewAsync(id, await GetCurrentAdminUserIdAsync(), reason);
                TempData["SuccessMessage"] = "Đã ẩn đánh giá.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            try
            {
                await _productReviewService.RejectReviewAsync(id, await GetCurrentAdminUserIdAsync(), reason);
                TempData["SuccessMessage"] = "Đã từ chối đánh giá.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, string content)
        {
            try
            {
                await _productReviewService.ReplyToReviewAsync(id, content, await GetCurrentAdminUserIdAsync());
                TempData["SuccessMessage"] = "Đã gửi phản hồi cho khách hàng.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<int> GetCurrentAdminUserIdAsync()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (adminUser == null)
            {
                throw new InvalidOperationException("Không tìm thấy tài khoản quản trị hiện tại.");
            }

            return adminUser.Id;
        }
    }
}
