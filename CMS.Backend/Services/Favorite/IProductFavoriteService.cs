using System.Collections.Generic;
using System.Threading.Tasks;
using CMS.Backend.Models.Favorite;

namespace CMS.Backend.Services.Favorite
{
    public interface IProductFavoriteService
    {
        Task<bool> AddFavoriteAsync(int customerId, int productId);
        Task<bool> RemoveFavoriteAsync(int customerId, int productId);
        Task<bool> IsFavoriteAsync(int customerId, int productId);
        Task<FavoriteListResponse> GetFavoritesAsync(int customerId, int page, int pageSize, string keyword = null);
        Task<HashSet<int>> GetFavoriteProductIdsAsync(int customerId, IEnumerable<int> productIds);
        Task<int> GetFavoriteCountAsync(int customerId);
    }
}
