using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // MVC Controller cho trang Nhat ky gui email trong Admin Panel
    [Authorize(Roles = "Admin,Staff")]
    public class EmailLogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailLogController> _logger;

        public EmailLogController(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<EmailLogController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            string? keyword,
            string? emailType,
            string? status,
            int page = 1)
        {
            int pageSize = 20;
            var query = _context.EmailLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(e =>
                    (e.RecipientEmail != null && e.RecipientEmail.Contains(keyword)) ||
                    (e.Subject != null && e.Subject.Contains(keyword)) ||
                    (e.ReferenceId != null && e.ReferenceId.ToString()!.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(emailType))
                query = query.Where(e => e.EmailType == emailType);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(e => e.Status == status);

            var totalItems = await query.CountAsync();
            var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["Title"] = "Nhat ky gui Email";
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Keyword = keyword;
            ViewBag.EmailType = emailType;
            ViewBag.Status = status;

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Retry(int id)
        {
            var emailLog = await _context.EmailLogs.FindAsync(id);
            if (emailLog == null)
            {
                TempData["ErrorMessage"] = "Khong tim thay ban ghi email log.";
                return RedirectToAction("Index");
            }

            if (emailLog.Status == "Sent")
            {
                TempData["ErrorMessage"] = "Email nay da gui thanh cong, khong can gui lai.";
                return RedirectToAction("Index");
            }

            emailLog.RetryCount++;
            emailLog.Status = "Pending";

            try
            {
                var sent = await _emailService.SendEmailAsync(
                    emailLog.RecipientEmail,
                    emailLog.RecipientName ?? "",
                    emailLog.Subject,
                    $"<p>Day la email gui lai tu he thong TuanTaiCMS. Ma tham chieu: {emailLog.ReferenceType} #{emailLog.ReferenceId}</p>",
                    default);

                if (sent)
                {
                    emailLog.Status = "Sent";
                    emailLog.SentAt = DateTime.Now;
                    TempData["SuccessMessage"] = $"Gui lai email #{id} thanh cong!";
                }
                else
                {
                    emailLog.Status = "Failed";
                    emailLog.ErrorMessage = "EmailService returned false on retry";
                    TempData["ErrorMessage"] = $"Gui lai email #{id} that bai.";
                }
            }
            catch (Exception ex)
            {
                emailLog.Status = "Failed";
                emailLog.ErrorMessage = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                _logger.LogError(ex, "Loi gui lai email #{Id}", id);
                TempData["ErrorMessage"] = $"Loi gui lai email #{id}: {ex.Message}";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
