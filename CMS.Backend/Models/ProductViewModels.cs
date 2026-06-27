using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMS.Backend.Models
{
    public class ProductActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool HasOrderHistory { get; set; }
        public bool CanSoftDelete { get; set; }
        public bool CanPermanentDelete { get; set; }
    }

    public class BulkProductActionResult
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> FailedProducts { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class ProductTrashItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public int StockQuantity { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeleteReason { get; set; }
        public bool HasOrderHistory { get; set; }
        public bool CanPermanentDelete { get; set; }
    }

    public class ProductTrashViewModel
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public string? DeletedTime { get; set; }
        public string? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<ProductTrashItemViewModel> Items { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
    }
}
