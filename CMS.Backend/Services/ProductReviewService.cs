using System.Text;
using CMS.Data;
using CMS.Data.Entities;
using CMS.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CMS.Backend.Services
{
    public class ProductReviewService : IProductReviewService
    {
        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        private const long MaxImageSizeBytes = 5 * 1024 * 1024;
        private const int MaxImageCount = 5;

        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _environment;

        public ProductReviewService(
            ApplicationDbContext context,
            INotificationService notificationService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _notificationService = notificationService;
            _environment = environment;
        }

        public async Task<ReviewEligibilityResult> CheckEligibilityAsync(int orderDetailId, int customerId)
        {
            var orderDetail = await _context.OrderDetails
                .AsNoTracking()
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Include(od => od.ProductReview)
                .FirstOrDefaultAsync(od => od.Id == orderDetailId);

            if (orderDetail == null || orderDetail.Order == null || orderDetail.Product == null)
            {
                return new ReviewEligibilityResult
                {
                    CanReview = false,
                    Message = "Khong tim thay san pham trong don hang."
                };
            }

            if (orderDetail.Order.CustomerId != customerId)
            {
                return new ReviewEligibilityResult
                {
                    CanReview = false,
                    Message = "Ban khong co quyen danh gia san pham nay."
                };
            }

            if (!CanReviewOrder(orderDetail.Order))
            {
                return new ReviewEligibilityResult
                {
                    CanReview = false,
                    Message = "Don hang chua hoan thanh nen chua the danh gia."
                };
            }

            if (orderDetail.ProductReview != null)
            {
                return new ReviewEligibilityResult
                {
                    CanReview = false,
                    AlreadyReviewed = true,
                    ExistingReviewId = orderDetail.ProductReview.Id,
                    Message = "San pham nay da duoc danh gia."
                };
            }

            return new ReviewEligibilityResult
            {
                CanReview = true,
                Message = "Ban co the danh gia san pham nay."
            };
        }

        public async Task<ProductReviewDto> CreateReviewAsync(CreateProductReviewRequest request, int customerId)
        {
            ValidateCreateRequest(request);

            var eligibility = await CheckEligibilityAsync(request.OrderDetailId, customerId);
            if (!eligibility.CanReview)
            {
                throw new InvalidOperationException(eligibility.Message);
            }

            var orderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .FirstAsync(od => od.Id == request.OrderDetailId);

            var storedImages = await SaveImagesAsync(request.Images);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var review = new ProductReview
                {
                    ProductId = orderDetail.ProductId,
                    OrderId = orderDetail.OrderId,
                    OrderDetailId = orderDetail.Id,
                    CustomerId = customerId,
                    Rating = request.Rating,
                    Title = NormalizeOptionalText(request.Title),
                    Content = request.Content.Trim(),
                    Status = ReviewStatus.Pending,
                    IsVerifiedPurchase = true,
                    CreatedAt = DateTime.Now,
                    Images = storedImages
                        .Select((path, index) => new ProductReviewImage
                        {
                            ImageUrl = path,
                            DisplayOrder = index
                        })
                        .ToList()
                };

                _context.ProductReviews.Add(review);
                await _context.SaveChangesAsync();

                var customer = await _context.Customers.AsNoTracking().FirstAsync(c => c.Id == customerId);
                var productName = orderDetail.Product?.Name ?? $"San pham #{orderDetail.ProductId}";

                await _notificationService.CreateForAllAdminsAsync(
                    $"Đánh giá mới cho {productName}",
                    $"{customer.FullName} vừa gửi đánh giá mới cho đơn hàng #{orderDetail.OrderId}.",
                    "ProductReviewPending",
                    "ProductReview",
                    review.Id);

                await transaction.CommitAsync();

                return await GetReviewDtoQuery(customerId)
                    .FirstAsync(r => r.Id == review.Id);
            }
            catch
            {
                DeleteFiles(storedImages);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PagedResult<ProductReviewDto>> GetProductReviewsAsync(int productId, ProductReviewFilter filter)
        {
            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Clamp(filter.PageSize, 1, 20);

            var query = _context.ProductReviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Published)
                .Include(r => r.Customer)
                .Include(r => r.Images)
                .Include(r => r.Replies)
                    .ThenInclude(reply => reply.AdminUser)
                .AsQueryable();

            if (filter.Rating.HasValue)
            {
                query = query.Where(r => r.Rating == filter.Rating.Value);
            }

            if (filter.HasImages == true)
            {
                query = query.Where(r => r.Images.Any());
            }

            query = filter.SortBy?.ToLowerInvariant() switch
            {
                "oldest" => query.OrderBy(r => r.CreatedAt),
                "rating-desc" => query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                "rating-asc" => query.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToProductReviewDto())
                .ToListAsync();

            return CreatePagedResult(items, page, pageSize, totalItems);
        }

        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(int productId)
        {
            var publishedReviews = _context.ProductReviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Published);

            var total = await publishedReviews.CountAsync();
            if (total == 0)
            {
                return new ProductReviewSummaryDto();
            }

            var stats = await publishedReviews
                .GroupBy(_ => 1)
                .Select(g => new ProductReviewSummaryDto
                {
                    AverageRating = Math.Round(g.Average(x => x.Rating), 1),
                    TotalReviews = g.Count(),
                    FiveStarCount = g.Count(x => x.Rating == 5),
                    FourStarCount = g.Count(x => x.Rating == 4),
                    ThreeStarCount = g.Count(x => x.Rating == 3),
                    TwoStarCount = g.Count(x => x.Rating == 2),
                    OneStarCount = g.Count(x => x.Rating == 1)
                })
                .FirstAsync();

            return stats;
        }

        public async Task<PagedResult<MyProductReviewDto>> GetMyReviewsAsync(int customerId, MyReviewFilter filter)
        {
            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Clamp(filter.PageSize, 1, 20);

            var query = _context.ProductReviews
                .AsNoTracking()
                .Where(r => r.CustomerId == customerId)
                .Include(r => r.Product)
                .Include(r => r.Images)
                .Include(r => r.Replies)
                    .ThenInclude(reply => reply.AdminUser)
                .OrderByDescending(r => r.CreatedAt)
                .AsQueryable();

            if (filter.Status.HasValue)
            {
                query = query.Where(r => r.Status == filter.Status.Value);
            }

            if (filter.HasReply.HasValue)
            {
                query = filter.HasReply.Value
                    ? query.Where(r => r.Replies.Any())
                    : query.Where(r => !r.Replies.Any());
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new MyProductReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    OrderId = r.OrderId,
                    OrderDetailId = r.OrderDetailId,
                    ProductName = r.Product != null ? r.Product.Name : $"San pham #{r.ProductId}",
                    UserDisplayName = string.Empty,
                    Rating = r.Rating,
                    Title = r.Title,
                    Content = r.Content,
                    IsVerifiedPurchase = r.IsVerifiedPurchase,
                    IsEdited = r.IsEdited,
                    Status = r.Status,
                    ModerationReason = r.ModerationReason,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Images = r.Images
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => new ProductReviewImageDto
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl
                        })
                        .ToList(),
                    Replies = r.Replies
                        .OrderBy(reply => reply.CreatedAt)
                        .Select(reply => new ProductReviewReplyDto
                        {
                            Id = reply.Id,
                            AdminUserId = reply.AdminUserId,
                            AdminName = reply.AdminUser != null ? reply.AdminUser.FullName : "Quan tri vien",
                            Content = reply.Content,
                            IsOfficial = reply.IsOfficial,
                            CreatedAt = reply.CreatedAt,
                            UpdatedAt = reply.UpdatedAt
                        })
                        .ToList(),
                    ReviewStatusLabel = GetStatusLabel(r.Status)
                })
                .ToListAsync();

            return CreatePagedResult(items, page, pageSize, totalItems);
        }

        public async Task<PagedResult<AdminProductReviewDto>> GetAdminReviewsAsync(AdminReviewFilter filter)
        {
            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Clamp(filter.PageSize, 1, 50);

            var query = _context.ProductReviews
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .Include(r => r.Images)
                .Include(r => r.Replies)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();
                query = query.Where(r =>
                    (r.Title != null && r.Title.ToLower().Contains(keyword)) ||
                    r.Content.ToLower().Contains(keyword) ||
                    (r.Product != null && r.Product.Name.ToLower().Contains(keyword)) ||
                    (r.Customer != null && r.Customer.FullName.ToLower().Contains(keyword)) ||
                    (r.Customer != null && r.Customer.Email.ToLower().Contains(keyword)));
            }

            if (filter.ProductId.HasValue)
            {
                query = query.Where(r => r.ProductId == filter.ProductId.Value);
            }

            if (filter.Rating.HasValue)
            {
                query = query.Where(r => r.Rating == filter.Rating.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(r => r.Status == filter.Status.Value);
            }

            if (filter.HasReply.HasValue)
            {
                query = filter.HasReply.Value
                    ? query.Where(r => r.Replies.Any())
                    : query.Where(r => !r.Replies.Any());
            }

            if (filter.FromDate.HasValue)
            {
                var fromDate = filter.FromDate.Value.Date;
                query = query.Where(r => r.CreatedAt >= fromDate);
            }

            if (filter.ToDate.HasValue)
            {
                var toDate = filter.ToDate.Value.Date.AddDays(1);
                query = query.Where(r => r.CreatedAt < toDate);
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new AdminProductReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ProductName = r.Product != null ? r.Product.Name : $"San pham #{r.ProductId}",
                    ProductImageUrl = r.Product != null ? r.Product.ImageUrl : null,
                    OrderId = r.OrderId,
                    CustomerName = r.Customer != null ? r.Customer.FullName : "Khach hang",
                    CustomerEmail = r.Customer != null ? r.Customer.Email : string.Empty,
                    CustomerAvatar = r.Customer != null ? r.Customer.AvatarUrl : null,
                    Rating = r.Rating,
                    Title = r.Title,
                    Content = r.Content,
                    Status = r.Status,
                    ImageCount = r.Images.Count,
                    HasReply = r.Replies.Any(),
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return CreatePagedResult(items, page, pageSize, totalItems);
        }

        public async Task<AdminProductReviewDetailDto?> GetAdminReviewDetailAsync(int reviewId)
        {
            return await _context.ProductReviews
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(r => r.Id == reviewId)
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .Include(r => r.Images)
                .Include(r => r.Replies)
                    .ThenInclude(reply => reply.AdminUser)
                .Select(r => new AdminProductReviewDetailDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    OrderId = r.OrderId,
                    OrderDetailId = r.OrderDetailId,
                    CustomerId = r.CustomerId,
                    ProductName = r.Product != null ? r.Product.Name : $"San pham #{r.ProductId}",
                    UserDisplayName = r.Customer != null ? r.Customer.FullName : "Khach hang",
                    UserAvatar = r.Customer != null ? r.Customer.AvatarUrl : null,
                    CustomerEmail = r.Customer != null ? r.Customer.Email : string.Empty,
                    CustomerPhone = r.Customer != null ? r.Customer.Phone : null,
                    Rating = r.Rating,
                    Title = r.Title,
                    Content = r.Content,
                    IsVerifiedPurchase = r.IsVerifiedPurchase,
                    IsEdited = r.IsEdited,
                    Status = r.Status,
                    ModerationReason = r.ModerationReason,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Images = r.Images
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => new ProductReviewImageDto
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl
                        })
                        .ToList(),
                    Replies = r.Replies
                        .OrderBy(reply => reply.CreatedAt)
                        .Select(reply => new ProductReviewReplyDto
                        {
                            Id = reply.Id,
                            AdminUserId = reply.AdminUserId,
                            AdminName = reply.AdminUser != null ? reply.AdminUser.FullName : "Quan tri vien",
                            Content = reply.Content,
                            IsOfficial = reply.IsOfficial,
                            CreatedAt = reply.CreatedAt,
                            UpdatedAt = reply.UpdatedAt
                        })
                        .ToList(),
                    AdminReplyCount = r.Replies.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task PublishReviewAsync(int reviewId, int adminUserId)
        {
            var review = await GetReviewForModerationAsync(reviewId);
            review.Status = ReviewStatus.Published;
            review.ModerationReason = null;
            review.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await _notificationService.CreateForCustomerAsync(
                "Đánh giá của bạn đã được công khai",
                "Đánh giá sản phẩm của bạn đã được duyệt và hiển thị trên trang chi tiết sản phẩm.",
                "ProductReviewPublished",
                review.CustomerId,
                "ProductReview",
                review.Id);
        }

        public async Task HideReviewAsync(int reviewId, int adminUserId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new InvalidOperationException("Vui long nhap ly do an danh gia.");
            }

            var review = await GetReviewForModerationAsync(reviewId);
            review.Status = ReviewStatus.Hidden;
            review.ModerationReason = reason.Trim();
            review.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await _notificationService.CreateForCustomerAsync(
                "Đánh giá của bạn tạm thời bị ẩn",
                $"Đánh giá sản phẩm của bạn đang được tạm ẩn. Lý do: {review.ModerationReason}",
                "ProductReviewHidden",
                review.CustomerId,
                "ProductReview",
                review.Id);
        }

        public async Task RejectReviewAsync(int reviewId, int adminUserId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new InvalidOperationException("Vui long nhap ly do tu choi danh gia.");
            }

            var review = await GetReviewForModerationAsync(reviewId);
            review.Status = ReviewStatus.Rejected;
            review.ModerationReason = reason.Trim();
            review.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await _notificationService.CreateForCustomerAsync(
                "Đánh giá của bạn bị từ chối",
                $"Đánh giá sản phẩm của bạn chưa được đăng. Lý do: {review.ModerationReason}",
                "ProductReviewRejected",
                review.CustomerId,
                "ProductReview",
                review.Id);
        }

        public async Task<ProductReviewReplyDto> ReplyToReviewAsync(int reviewId, string content, int adminUserId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("Noi dung phan hoi khong duoc de trong.");
            }

            var review = await _context.ProductReviews
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == reviewId)
                ?? throw new KeyNotFoundException("Khong tim thay danh gia.");

            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminUserId)
                ?? throw new InvalidOperationException("Khong tim thay tai khoan quan tri.");

            var reply = new ProductReviewReply
            {
                ProductReviewId = reviewId,
                AdminUserId = adminUserId,
                Content = content.Trim(),
                IsOfficial = true,
                CreatedAt = DateTime.Now
            };

            _context.ProductReviewReplies.Add(reply);

            if (review.Status == ReviewStatus.Pending)
            {
                review.Status = ReviewStatus.Published;
            }

            review.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            await _notificationService.CreateForCustomerAsync(
                "Cửa hàng đã phản hồi đánh giá của bạn",
                "Bạn đã nhận được phản hồi mới từ cửa hàng cho đánh giá sản phẩm.",
                "ProductReviewReply",
                review.CustomerId,
                "ProductReview",
                review.Id);

            return new ProductReviewReplyDto
            {
                Id = reply.Id,
                AdminUserId = reply.AdminUserId,
                AdminName = admin.FullName,
                Content = reply.Content,
                IsOfficial = reply.IsOfficial,
                CreatedAt = reply.CreatedAt,
                UpdatedAt = reply.UpdatedAt
            };
        }

        public async Task<ProductReviewDto?> GetReviewByIdAsync(int reviewId, int customerId)
        {
            return await GetReviewDtoQuery(customerId)
                .FirstOrDefaultAsync(r => r.Id == reviewId);
        }

        private IQueryable<ProductReviewDto> GetReviewDtoQuery(int customerId)
        {
            return _context.ProductReviews
                .AsNoTracking()
                .Where(r => r.CustomerId == customerId)
                .Include(r => r.Product)
                .Include(r => r.Customer)
                .Include(r => r.Images)
                .Include(r => r.Replies)
                    .ThenInclude(reply => reply.AdminUser)
                .Select(MapToProductReviewDto());
        }

        private static Expression<Func<ProductReview, ProductReviewDto>> MapToProductReviewDto()
        {
            return r => new ProductReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                OrderId = r.OrderId,
                OrderDetailId = r.OrderDetailId,
                ProductName = r.Product != null ? r.Product.Name : $"San pham #{r.ProductId}",
                UserDisplayName = r.Customer != null ? r.Customer.FullName : "Khach hang",
                UserAvatar = r.Customer != null ? r.Customer.AvatarUrl : null,
                Rating = r.Rating,
                Title = r.Title,
                Content = r.Content,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                IsEdited = r.IsEdited,
                Status = r.Status,
                ModerationReason = r.ModerationReason,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Images = r.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new ProductReviewImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl
                    })
                    .ToList(),
                Replies = r.Replies
                    .OrderBy(reply => reply.CreatedAt)
                    .Select(reply => new ProductReviewReplyDto
                    {
                        Id = reply.Id,
                        AdminUserId = reply.AdminUserId,
                        AdminName = reply.AdminUser != null ? reply.AdminUser.FullName : "Quan tri vien",
                        Content = reply.Content,
                        IsOfficial = reply.IsOfficial,
                        CreatedAt = reply.CreatedAt,
                        UpdatedAt = reply.UpdatedAt
                    })
                    .ToList()
            };
        }

        private async Task<ProductReview> GetReviewForModerationAsync(int reviewId)
        {
            return await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId)
                ?? throw new KeyNotFoundException("Khong tim thay danh gia.");
        }

        private static bool CanReviewOrder(Order order)
        {
            return order.Status == (int)OrderStatus.COMPLETED;
        }

        private static string NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static void ValidateCreateRequest(CreateProductReviewRequest request)
        {
            if (request.OrderDetailId <= 0)
            {
                throw new InvalidOperationException("Khong tim thay san pham trong don hang.");
            }

            if (request.Rating < 1 || request.Rating > 5)
            {
                throw new InvalidOperationException("Vui long chon so sao.");
            }

            if (!string.IsNullOrWhiteSpace(request.Title) && request.Title.Trim().Length > 150)
            {
                throw new InvalidOperationException("Tieu de danh gia khong duoc vuot qua 150 ky tu.");
            }

            var content = request.Content?.Trim() ?? string.Empty;
            if (content.Length < 10)
            {
                throw new InvalidOperationException("Noi dung danh gia phai co it nhat 10 ky tu.");
            }

            if (content.Length > 2000)
            {
                throw new InvalidOperationException("Noi dung danh gia khong duoc vuot qua 2000 ky tu.");
            }

            if (request.Images != null && request.Images.Count > MaxImageCount)
            {
                throw new InvalidOperationException("Ban chi duoc tai toi da 5 hinh anh.");
            }
        }

        private async Task<List<string>> SaveImagesAsync(List<IFormFile>? files)
        {
            var storedFiles = new List<string>();

            if (files == null || files.Count == 0)
            {
                return storedFiles;
            }

            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "reviews");
            Directory.CreateDirectory(uploadFolder);

            foreach (var file in files)
            {
                ValidateImage(file);

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadFolder, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                storedFiles.Add($"/uploads/reviews/{fileName}");
            }

            return storedFiles;
        }

        private static void ValidateImage(IFormFile file)
        {
            if (file.Length <= 0)
            {
                throw new InvalidOperationException("Khong the tai len tep rong.");
            }

            if (file.Length > MaxImageSizeBytes)
            {
                throw new InvalidOperationException("Moi hinh anh khong duoc vuot qua 5 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Dinh dang hinh anh khong hop le.");
            }

            var contentType = file.ContentType.ToLowerInvariant();
            if (!AllowedContentTypes.Contains(contentType))
            {
                throw new InvalidOperationException("Dinh dang hinh anh khong hop le.");
            }

            using var stream = file.OpenReadStream();
            if (!MatchesFileSignature(stream, extension))
            {
                throw new InvalidOperationException("Dinh dang hinh anh khong hop le.");
            }
        }

        private static bool MatchesFileSignature(Stream stream, string extension)
        {
            var buffer = new byte[12];
            var read = stream.Read(buffer, 0, buffer.Length);
            stream.Position = 0;

            if (read < 4)
            {
                return false;
            }

            return extension switch
            {
                ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8,
                ".png" => buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
                ".webp" => read >= 12
                    && Encoding.ASCII.GetString(buffer, 0, 4) == "RIFF"
                    && Encoding.ASCII.GetString(buffer, 8, 4) == "WEBP",
                _ => false
            };
        }

        private void DeleteFiles(IEnumerable<string> relativePaths)
        {
            foreach (var relativePath in relativePaths)
            {
                var normalized = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(_environment.WebRootPath, normalized);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
        }

        private static PagedResult<T> CreatePagedResult<T>(List<T> items, int page, int pageSize, int totalItems)
        {
            return new PagedResult<T>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        private static string GetStatusLabel(ReviewStatus status)
        {
            return status switch
            {
                ReviewStatus.Pending => "Cho duyet",
                ReviewStatus.Published => "Da cong khai",
                ReviewStatus.Hidden => "Da an",
                ReviewStatus.Rejected => "Da tu choi",
                _ => "Khong xac dinh"
            };
        }
    }
}
