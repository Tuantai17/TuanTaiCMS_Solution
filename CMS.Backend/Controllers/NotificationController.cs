using CMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? type, bool? isRead, int page = 1)
        {
            int pageSize = 20;
            var query = _context.Notifications.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(n => n.NotificationType == type);

            if (isRead.HasValue)
                query = query.Where(n => n.IsRead == isRead.Value);

            var totalItems = await query.CountAsync();
            var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["Title"] = "Thong bao he thong";
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.FilterType = type;
            ViewBag.FilterIsRead = isRead;
            ViewBag.UnreadCount = await _context.Notifications.CountAsync(n => !n.IsRead);

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            var unread = await _context.Notifications.Where(n => !n.IsRead).ToListAsync();
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.Now;
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Da danh dau {unread.Count} thong bao da doc.";
            return RedirectToAction("Index");
        }
    }
}
