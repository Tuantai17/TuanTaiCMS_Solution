using CMS.Backend.Helpers;
using CMS.Backend.Models;
using CMS.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Backend.Controllers;

[Route("api/support/tickets")]
[ApiController]
public class SupportTicketsController : ControllerBase
{
    private readonly ISupportTicketService _supportTicketService;
    private readonly IConfiguration _configuration;

    public SupportTicketsController(ISupportTicketService supportTicketService, IConfiguration configuration)
    {
        _supportTicketService = supportTicketService;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyTickets(
        [FromQuery] string? keyword,
        [FromQuery] string? status,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
        {
            return authError!;
        }

        var result = await _supportTicketService.GetCustomerTicketsAsync(customerId, new SupportTicketCustomerFilter
        {
            Keyword = keyword,
            Status = status,
            Category = category,
            Page = page,
            PageSize = pageSize
        });

        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
        {
            return authError!;
        }

        var count = await _supportTicketService.GetCustomerUnreadTicketCountAsync(customerId);
        return Ok(new { count });
    }

    [HttpGet("{ticketId}")]
    public async Task<IActionResult> GetMyTicketDetail(string ticketId)
    {
        if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
        {
            return authError!;
        }

        var ticket = await _supportTicketService.GetCustomerTicketDetailAsync(ticketId, customerId);
        if (ticket == null)
        {
            return NotFound(new { message = "Không tìm thấy yêu cầu hỗ trợ." });
        }

        return Ok(ticket);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateTicket([FromForm] CreateSupportTicketRequest request)
    {
        if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
        {
            return authError!;
        }

        try
        {
            var createdTicket = await _supportTicketService.CreateTicketAsync(request, customerId);
            return Ok(createdTicket);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{ticketId}/messages")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddMessage(string ticketId, [FromForm] CreateSupportTicketMessageRequest request)
    {
        if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
        {
            return authError!;
        }

        try
        {
            var updatedTicket = await _supportTicketService.AddCustomerMessageAsync(ticketId, request, customerId);
            return Ok(updatedTicket);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{ticketId}/read")]
    public async Task<IActionResult> MarkAsRead(string ticketId)
    {
        if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
        {
            return authError!;
        }

        var updatedTicket = await _supportTicketService.GetCustomerTicketDetailAsync(ticketId, customerId, markAsRead: true);
        if (updatedTicket == null)
        {
            return NotFound(new { message = "Không tìm thấy yêu cầu hỗ trợ." });
        }

        return Ok(updatedTicket);
    }

    [HttpPost("{ticketId}/reopen")]
    public async Task<IActionResult> Reopen(string ticketId)
    {
        if (!TryGetAuthenticatedCustomerId(out var customerId, out var authError))
        {
            return authError!;
        }

        try
        {
            var updatedTicket = await _supportTicketService.ReopenTicketAsync(ticketId, customerId);
            return Ok(updatedTicket);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private bool TryGetAuthenticatedCustomerId(out int customerId, out IActionResult? errorResult)
    {
        customerId = 0;
        errorResult = null;

        var authorizationHeader = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            errorResult = Unauthorized(new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            return false;
        }

        var token = authorizationHeader["Bearer ".Length..].Trim();
        var secret = _configuration["CustomerSession:Secret"] ?? "TuanTaiCMS.CustomerSession.Secret.2026";
        if (!CustomerSessionTokenHelper.TryValidateToken(token, secret, out customerId))
        {
            errorResult = Unauthorized(new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            return false;
        }

        return true;
    }
}
