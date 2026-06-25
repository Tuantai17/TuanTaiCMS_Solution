using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    [Route("api/customer-notifications")]
    [ApiController]
    public class CustomerNotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CustomerNotificationsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Helper: Lấy ID khách hàng từ JWT token tuỳ chỉnh
        private int GetCurrentCustomerId()
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var secret = _configuration["CustomerSession:Secret"] ?? "TuanTaiCMS.CustomerSession.Secret.2026";
            
            if (!string.IsNullOrEmpty(token) && CMS.Backend.Helpers.CustomerSessionTokenHelper.TryValidateToken(token, secret, out var customerId))
            {
                return customerId;
            }
            return 0; // Invalid
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            int customerId = GetCurrentCustomerId();
            if (customerId <= 0) return Unauthorized(new { message = "Khong the xac thuc nguoi dung." });

            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => n.TargetCustomerId == customerId);

            var totalItems = await query.CountAsync();
            var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { items, page, pageSize, totalItems, totalPages });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            int customerId = GetCurrentCustomerId();
            if (customerId <= 0) return Unauthorized(new { message = "Khong the xac thuc nguoi dung." });

            var count = await _context.Notifications.CountAsync(n => n.TargetCustomerId == customerId && !n.IsRead);
            return Ok(new { count });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            int customerId = GetCurrentCustomerId();
            if (customerId <= 0) return Unauthorized(new { message = "Khong the xac thuc nguoi dung." });

            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.TargetCustomerId == customerId);
            if (notification == null) return NotFound(new { message = "Khong tim thay thong bao hoac ban khong co quyen truy cap." });

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Da danh dau da doc" });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int customerId = GetCurrentCustomerId();
            if (customerId <= 0) return Unauthorized(new { message = "Khong the xac thuc nguoi dung." });

            var unread = await _context.Notifications
                .Where(n => n.TargetCustomerId == customerId && !n.IsRead)
                .ToListAsync();

            if (unread.Any())
            {
                foreach (var n in unread)
                {
                    n.IsRead = true;
                    n.ReadAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new { message = $"Da danh dau {unread.Count} thong bao da doc" });
        }
    }
}
