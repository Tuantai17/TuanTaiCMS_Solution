using CMS.Backend.Models;

namespace CMS.Backend.Services
{
    public interface IProductService
    {
        Task<bool> HasOrderHistoryAsync(int productId);
        Task<ProductActionResult> SoftDeleteAsync(int productId, string deletedBy, string? reason);
        Task<BulkProductActionResult> BulkSoftDeleteAsync(IEnumerable<int> productIds, string deletedBy, string? reason);
        Task<ProductActionResult> RestoreAsync(int productId);
        Task<BulkProductActionResult> BulkRestoreAsync(IEnumerable<int> productIds);
        Task<ProductActionResult> PermanentDeleteAsync(int productId);
        Task<BulkProductActionResult> BulkPermanentDeleteAsync(IEnumerable<int> productIds);
        Task<ProductTrashViewModel> GetTrashAsync(ProductTrashViewModel filter);
        Task<int> GetTrashCountAsync();
    }
}
