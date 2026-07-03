using CMS.Backend.Models;
using CMS.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Backend.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class SupportTicketController : Controller
{
    private readonly ISupportTicketService _supportTicketService;

    public SupportTicketController(ISupportTicketService supportTicketService)
    {
        _supportTicketService = supportTicketService;
    }

    public async Task<IActionResult> Index(string? keyword, string? status, string? category, string? priority, int page = 1, int pageSize = 10)
    {
        var filter = new SupportTicketAdminFilterViewModel
        {
            Keyword = keyword ?? string.Empty,
            Status = status ?? string.Empty,
            Category = category ?? string.Empty,
            Priority = priority ?? string.Empty,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Hỗ trợ khách hàng";
        var model = await _supportTicketService.GetAdminTicketsAsync(filter);
        return View(model);
    }

    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var model = await _supportTicketService.GetAdminTicketDetailAsync(id, markAsRead: true);
        if (model == null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Chi tiết yêu cầu hỗ trợ";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(string id, string content)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(content))
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var userName = User.Identity?.Name ?? "Admin";
        await _supportTicketService.SendAdminReplyAsync(id, content, userName);

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string id, string status)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(status))
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        await _supportTicketService.UpdateAdminTicketStatusAsync(id, status);

        return RedirectToAction(nameof(Details), new { id });
    }
}
