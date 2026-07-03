using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CMS.Backend.Models;

public class SupportTicketCustomerFilter
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public string? Category { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CreateSupportTicketRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? RelatedOrderId { get; set; }
    public string? RelatedProductId { get; set; }
    public string? StickerCode { get; set; }
    public List<IFormFile>? Images { get; set; }
}

public class CreateSupportTicketMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public string? StickerCode { get; set; }
    public List<IFormFile>? Images { get; set; }
}

public class SupportTicketCustomerMessageViewModel
{
    public string Id { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderType { get; set; } = string.Empty; // "customer" or "staff"
    public string SenderRole { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? StickerCode { get; set; }
    public List<SupportTicketAttachmentViewModel> Attachments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class SupportTicketAttachmentViewModel
{
    public string Url { get; set; } = string.Empty;
}

public class SupportTicketCustomerItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string LastMessagePreview { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? RelatedOrderId { get; set; }
    public string? RelatedProductId { get; set; }
    
    // For detail view
    public List<SupportTicketCustomerMessageViewModel>? Messages { get; set; }
}

public class SupportTicketCustomerStatsViewModel
{
    public int All { get; set; }
    public int New { get; set; }
    public int InProgress { get; set; }
    public int WaitingCustomer { get; set; }
    public int Resolved { get; set; }
    public int Closed { get; set; }
    public int UnreadTickets { get; set; }
}

public class SupportTicketCustomerPagedResult
{
    public List<SupportTicketCustomerItemViewModel> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public SupportTicketCustomerStatsViewModel Stats { get; set; } = new();
}
