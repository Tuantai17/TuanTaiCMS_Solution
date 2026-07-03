namespace CMS.Backend.Models;

public class SupportTicketAdminFilterViewModel
{
    public string Keyword { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SupportTicketAdminStatsViewModel
{
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int InProgressCount { get; set; }
    public int WaitingCount { get; set; }
    public int ResolvedCount { get; set; }
}

public class SupportTicketAdminItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusTone { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string PriorityLabel { get; set; } = string.Empty;
    public string PriorityTone { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string RelatedOrderCode { get; set; } = string.Empty;
    public string RelatedProductName { get; set; } = string.Empty;
    public string ChannelLabel { get; set; } = "Website (mykingdom.com)";
}

public class SupportTicketAdminMessageViewModel
{
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string SenderType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? StickerCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SupportTicketAdminAttachmentViewModel> Attachments { get; set; } = [];
}

public class SupportTicketAdminAttachmentViewModel
{
    public string Url { get; set; } = string.Empty;
}

public class SupportTicketAdminListPageViewModel
{
    public SupportTicketAdminFilterViewModel Filter { get; set; } = new();
    public SupportTicketAdminStatsViewModel Stats { get; set; } = new();
    public List<SupportTicketAdminItemViewModel> Items { get; set; } = [];
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

public class SupportTicketAdminDetailPageViewModel
{
    public SupportTicketAdminItemViewModel Ticket { get; set; } = new();
    public List<SupportTicketAdminMessageViewModel> Messages { get; set; } = [];
}
