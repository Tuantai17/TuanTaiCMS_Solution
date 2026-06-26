using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CMS.Backend.Services.Favorite;
using CMS.Backend.Helpers;
using Microsoft.Extensions.Configuration;

namespace CMS.Backend.Controllers
{
    [ApiController]
    [Route("api/favorites")]
    public class FavoritesController : ControllerBase
    {
        private readonly IProductFavoriteService _favoriteService;
        private readonly IConfiguration _configuration;

        public FavoritesController(IProductFavoriteService favoriteService, IConfiguration configuration)
        {
            _favoriteService = favoriteService;
            _configuration = configuration;
        }

        private int GetCustomerId()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return 0;

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var secret = _configuration["CustomerSession:Secret"] ?? "TuanTaiCMS.CustomerSession.Secret.2026";
            
            if (CustomerSessionTokenHelper.TryValidateToken(token, secret, out int customerId))
            {
                return customerId;
            }
            return 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetFavorites([FromQuery] int page = 1, [FromQuery] int pageSize = 12, [FromQuery] string keyword = null)
        {
            var customerId = GetCustomerId();
            if (customerId <= 0) return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để sử dụng chức năng yêu thích." });

            var result = await _favoriteService.GetFavoritesAsync(customerId, page, pageSize, keyword);
            return Ok(result);
        }

        [HttpPost("{productId}")]
        public async Task<IActionResult> AddFavorite(int productId)
        {
            var customerId = GetCustomerId();
            if (customerId <= 0) return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để sử dụng chức năng yêu thích." });

            var success = await _favoriteService.AddFavoriteAsync(customerId, productId);
            if (!success)
            {
                // Có thể là sản phẩm không tồn tại
                return NotFound(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            return Ok(new { success = true, isFavorite = true, message = "Đã thêm sản phẩm vào danh sách yêu thích." });
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFavorite(int productId)
        {
            var customerId = GetCustomerId();
            if (customerId <= 0) return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để sử dụng chức năng yêu thích." });

            var success = await _favoriteService.RemoveFavoriteAsync(customerId, productId);
            if (!success)
            {
                // Có thể chưa yêu thích hoặc không tìm thấy
                return Ok(new { success = true, isFavorite = false, message = "Sản phẩm không nằm trong danh sách yêu thích." });
            }

            return Ok(new { success = true, isFavorite = false, message = "Đã xóa sản phẩm khỏi danh sách yêu thích." });
        }

        [HttpGet("{productId}/status")]
        public async Task<IActionResult> CheckFavoriteStatus(int productId)
        {
            var customerId = GetCustomerId();
            if (customerId <= 0) return Ok(new { isFavorite = false });

            var isFavorite = await _favoriteService.IsFavoriteAsync(customerId, productId);
            return Ok(new { isFavorite });
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetFavoriteCount()
        {
            var customerId = GetCustomerId();
            if (customerId <= 0) return Ok(new { count = 0 });

            var count = await _favoriteService.GetFavoriteCountAsync(customerId);
            return Ok(new { count });
        }
    }
}
