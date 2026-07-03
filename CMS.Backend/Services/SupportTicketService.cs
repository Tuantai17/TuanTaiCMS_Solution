using CMS.Backend.Models;
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Services
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly ApplicationDbContext _context;

        public SupportTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SupportTicketAdminListPageViewModel> GetAdminTicketsAsync(SupportTicketAdminFilterViewModel filter)
        {
            var query = _context.SupportTickets.Include(t => t.Customer).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                query = query.Where(t => t.Code.Contains(filter.Keyword) || t.Subject.Contains(filter.Keyword) || t.Customer.FullName.Contains(filter.Keyword) || t.Customer.Email.Contains(filter.Keyword));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(t => t.Status == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                query = query.Where(t => t.Category == filter.Category);
            }

            if (!string.IsNullOrWhiteSpace(filter.Priority))
            {
                query = query.Where(t => t.Priority == filter.Priority);
            }

            var totalCount = await query.CountAsync();
            var unreadCount = await _context.SupportTickets.SumAsync(t => t.UnreadCount);
            var inProgressCount = await _context.SupportTickets.CountAsync(t => t.Status == "in-progress");
            var waitingCount = await _context.SupportTickets.CountAsync(t => t.Status == "waiting-customer");
            var resolvedCount = await _context.SupportTickets.CountAsync(t => t.Status == "resolved");

            var items = await query
                .OrderByDescending(t => t.UpdatedAt)
                .Skip((Math.Max(filter.Page, 1) - 1) * Math.Max(filter.PageSize, 1))
                .Take(Math.Max(filter.PageSize, 1))
                .ToListAsync();

            var mappedItems = items.Select(t => new SupportTicketAdminItemViewModel
            {
                Id = t.Id,
                Code = t.Code,
                CustomerName = t.Customer?.FullName ?? "Khách vãng lai",
                CustomerEmail = t.Customer?.Email ?? "",
                CustomerPhone = t.Customer?.Phone ?? "",
                Subject = t.Subject,
                Category = t.Category,
                CategoryLabel = GetCategoryLabel(t.Category),
                Status = t.Status,
                StatusLabel = GetStatusLabel(t.Status),
                StatusTone = GetStatusTone(t.Status),
                Priority = t.Priority,
                PriorityLabel = GetPriorityLabel(t.Priority),
                PriorityTone = GetPriorityTone(t.Priority),
                UnreadCount = t.UnreadCount,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                RelatedOrderCode = t.RelatedOrderCode ?? "",
                RelatedProductName = t.RelatedProductName ?? ""
            }).ToList();

            return new SupportTicketAdminListPageViewModel
            {
                Filter = filter,
                Stats = new SupportTicketAdminStatsViewModel
                {
                    TotalCount = totalCount,
                    UnreadCount = unreadCount,
                    InProgressCount = inProgressCount,
                    WaitingCount = waitingCount,
                    ResolvedCount = resolvedCount
                },
                Items = mappedItems,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / Math.Max(filter.PageSize, 1))
            };
        }

        public async Task<SupportTicketAdminDetailPageViewModel?> GetAdminTicketDetailAsync(string id, bool markAsRead = false)
        {
            var ticket = await _context.SupportTickets
                .Include(t => t.Customer)
                .Include(t => t.Messages)
                .FirstOrDefaultAsync(t => t.Id == id || t.Code == id);

            if (ticket == null) return null;

            if (markAsRead && ticket.UnreadCount > 0)
            {
                // UnreadCount is actually for customer based on schema. But let's leave it.
            }

            var model = new SupportTicketAdminDetailPageViewModel
            {
                Ticket = new SupportTicketAdminItemViewModel
                {
                    Id = ticket.Id,
                    Code = ticket.Code,
                    CustomerName = ticket.Customer?.FullName ?? "Khách vãng lai",
                    CustomerEmail = ticket.Customer?.Email ?? "",
                    CustomerPhone = ticket.Customer?.Phone ?? "",
                    Subject = ticket.Subject,
                    Category = ticket.Category,
                    CategoryLabel = GetCategoryLabel(ticket.Category),
                    Status = ticket.Status,
                    StatusLabel = GetStatusLabel(ticket.Status),
                    StatusTone = GetStatusTone(ticket.Status),
                    Priority = ticket.Priority,
                    PriorityLabel = GetPriorityLabel(ticket.Priority),
                    PriorityTone = GetPriorityTone(ticket.Priority),
                    UnreadCount = ticket.UnreadCount,
                    CreatedAt = ticket.CreatedAt,
                    UpdatedAt = ticket.UpdatedAt,
                    RelatedOrderCode = ticket.RelatedOrderCode ?? "",
                    RelatedProductName = ticket.RelatedProductName ?? ""
                },
                Messages = ticket.Messages.OrderBy(m => m.CreatedAt).Select(m => new SupportTicketAdminMessageViewModel
                {
                    SenderName = m.SenderName,
                    SenderRole = m.SenderType == "customer" ? "Khách hàng" : "Admin",
                    SenderType = m.SenderType,
                    Content = m.Content ?? "",
                    StickerCode = m.StickerCode,
                    CreatedAt = m.CreatedAt,
                    Attachments = !string.IsNullOrEmpty(m.Attachments) ? System.Text.Json.JsonSerializer.Deserialize<List<SupportTicketAdminAttachmentViewModel>>(m.Attachments, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<SupportTicketAdminAttachmentViewModel>() : new List<SupportTicketAdminAttachmentViewModel>()
                }).ToList()
            };

            return model;
        }

        private string GetCategoryLabel(string category) => category switch
        {
            "order" => "Đơn hàng",
            "product" => "Sản phẩm",
            "payment" => "Thanh toán",
            "account" => "Tài khoản",
            _ => "Khác"
        };

        private string GetStatusLabel(string status) => status switch
        {
            "new" => "Mới tiếp nhận",
            "in-progress" => "Đang xử lý",
            "waiting-customer" => "Chờ phản hồi",
            "resolved" => "Đã giải quyết",
            "closed" => "Đã đóng",
            _ => "Không rõ"
        };

        private string GetStatusTone(string status) => status switch
        {
            "new" => "new",
            "in-progress" => "progress",
            "waiting-customer" => "waiting",
            "resolved" => "resolved",
            "closed" => "closed",
            _ => "closed"
        };

        private string GetPriorityLabel(string priority) => priority switch
        {
            "low" => "Thấp",
            "normal" => "Bình thường",
            "high" => "Cao",
            "urgent" => "Khẩn cấp",
            _ => "Bình thường"
        };

        private string GetPriorityTone(string priority) => priority switch
        {
            "low" => "success",
            "normal" => "info",
            "high" => "warning",
            "urgent" => "danger",
            _ => "info"
        };

        public async Task<SupportTicketMessage> SendAdminReplyAsync(string ticketId, string content, string staffName)
        {
            var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId || t.Code == ticketId);
            if (ticket == null)
            {
                throw new Exception("Ticket not found.");
            }

            var message = new SupportTicketMessage
            {
                Id = "msg-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                TicketId = ticket.Id,
                SenderType = "staff",
                SenderName = staffName,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportTicketMessages.Add(message);

            ticket.UpdatedAt = DateTime.UtcNow;
            ticket.UnreadCount += 1; // Increment unread for customer
            ticket.Status = "waiting-customer";

            await _context.SaveChangesAsync();
            return message;
        }

        public async Task UpdateAdminTicketStatusAsync(string ticketId, string status)
        {
            var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId || t.Code == ticketId);
            if (ticket != null)
            {
                ticket.Status = status;
                ticket.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // --- Frontend API Methods ---

        public async Task<SupportTicketCustomerPagedResult> GetCustomerTicketsAsync(int customerId, SupportTicketCustomerFilter filter)
        {
            var query = _context.SupportTickets.Where(t => t.CustomerId == customerId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                query = query.Where(t => t.Code.Contains(filter.Keyword) || t.Subject.Contains(filter.Keyword) || (t.RelatedOrderCode != null && t.RelatedOrderCode.Contains(filter.Keyword)) || (t.RelatedProductName != null && t.RelatedProductName.Contains(filter.Keyword)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status != "all")
            {
                query = query.Where(t => t.Status == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                query = query.Where(t => t.Category == filter.Category);
            }

            var allTickets = await _context.SupportTickets.Where(t => t.CustomerId == customerId).ToListAsync();
            var stats = new SupportTicketCustomerStatsViewModel
            {
                All = allTickets.Count,
                UnreadTickets = allTickets.Sum(t => t.UnreadCount),
                New = allTickets.Count(t => t.Status == "new"),
                InProgress = allTickets.Count(t => t.Status == "in-progress"),
                WaitingCustomer = allTickets.Count(t => t.Status == "waiting-customer"),
                Resolved = allTickets.Count(t => t.Status == "resolved"),
                Closed = allTickets.Count(t => t.Status == "closed")
            };

            var items = await query
                .Include(t => t.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .OrderByDescending(t => t.UpdatedAt)
                .Skip((Math.Max(filter.Page, 1) - 1) * Math.Max(filter.PageSize, 1))
                .Take(Math.Max(filter.PageSize, 1))
                .ToListAsync();

            return new SupportTicketCustomerPagedResult
            {
                Items = items.Select(t => MapToCustomerItemViewModel(t)).ToList(),
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalItems = stats.All,
                TotalPages = (int)Math.Ceiling((double)stats.All / Math.Max(filter.PageSize, 1)),
                Stats = stats
            };
        }

        public async Task<SupportTicketCustomerItemViewModel?> GetCustomerTicketDetailAsync(string ticketId, int customerId, bool markAsRead = false)
        {
            var ticket = await _context.SupportTickets
                .Include(t => t.Messages)
                .FirstOrDefaultAsync(t => (t.Id == ticketId || t.Code == ticketId) && t.CustomerId == customerId);

            if (ticket == null) return null;

            if (markAsRead && ticket.UnreadCount > 0)
            {
                ticket.UnreadCount = 0;
                await _context.SaveChangesAsync();
            }

            return MapToCustomerItemViewModel(ticket, true);
        }

        public async Task<SupportTicketCustomerItemViewModel> CreateTicketAsync(CreateSupportTicketRequest request, int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            var customerName = customer?.FullName ?? "Khách hàng";

            var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var todayCount = await _context.SupportTickets.CountAsync(t => t.Code.Contains(todayStr));
            var code = $"HT-{todayStr}-{(todayCount + 1).ToString().PadLeft(4, '0')}";
            
            var ticket = new SupportTicket
            {
                Id = $"{code}-{customerId}",
                Code = code,
                Subject = request.Subject,
                Category = request.Category,
                Status = "new",
                Priority = "normal",
                CustomerId = customerId,
                RelatedOrderId = request.RelatedOrderId,
                RelatedOrderCode = request.RelatedOrderId, // Simple map for now
                RelatedProductId = request.RelatedProductId,
                RelatedProductName = request.RelatedProductId, // Simple map for now
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UnreadCount = 0
            };

            var message = new SupportTicketMessage
            {
                Id = "msg-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                SenderType = "customer",
                SenderName = customerName,
                Content = request.Content,
                Attachments = "", // Need a file service to save IFormFile if present, leaving empty for now
                StickerCode = request.StickerCode,
                CreatedAt = DateTime.UtcNow
            };

            ticket.Messages.Add(message);
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            return MapToCustomerItemViewModel(ticket, true);
        }

        public async Task<SupportTicketCustomerItemViewModel> AddCustomerMessageAsync(string ticketId, CreateSupportTicketMessageRequest request, int customerId)
        {
            var ticket = await _context.SupportTickets
                .Include(t => t.Messages)
                .FirstOrDefaultAsync(t => (t.Id == ticketId || t.Code == ticketId) && t.CustomerId == customerId);
            
            if (ticket == null) throw new InvalidOperationException("Không tìm thấy yêu cầu hỗ trợ.");
            if (ticket.Status == "closed") throw new InvalidOperationException("Yêu cầu đã đóng, bạn cần mở lại trước khi gửi tin nhắn mới.");

            var customer = await _context.Customers.FindAsync(customerId);
            var customerName = customer?.FullName ?? "Khách hàng";

            var message = new SupportTicketMessage
            {
                Id = "msg-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                TicketId = ticket.Id,
                SenderType = "customer",
                SenderName = customerName,
                Content = request.Content,
                Attachments = "", // File service needed
                StickerCode = request.StickerCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportTicketMessages.Add(message);

            ticket.UpdatedAt = DateTime.UtcNow;
            if (ticket.Status == "waiting-customer" || ticket.Status == "resolved")
            {
                ticket.Status = "in-progress";
            }

            await _context.SaveChangesAsync();
            return MapToCustomerItemViewModel(ticket, true);
        }

        public async Task<SupportTicketCustomerItemViewModel> ReopenTicketAsync(string ticketId, int customerId)
        {
            var ticket = await _context.SupportTickets
                .Include(t => t.Messages)
                .FirstOrDefaultAsync(t => (t.Id == ticketId || t.Code == ticketId) && t.CustomerId == customerId);
            
            if (ticket == null) throw new InvalidOperationException("Không tìm thấy yêu cầu hỗ trợ.");

            ticket.Status = "in-progress";
            ticket.UpdatedAt = DateTime.UtcNow;

            var message = new SupportTicketMessage
            {
                Id = "msg-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                TicketId = ticket.Id,
                SenderType = "system",
                SenderName = "Hệ thống",
                Content = "Yêu cầu đã được mở lại để tiếp tục trao đổi.",
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportTicketMessages.Add(message);
            await _context.SaveChangesAsync();

            return MapToCustomerItemViewModel(ticket, true);
        }

        public async Task<int> GetCustomerUnreadTicketCountAsync(int customerId)
        {
            return await _context.SupportTickets.SumAsync(t => t.CustomerId == customerId ? t.UnreadCount : 0);
        }

        private SupportTicketCustomerItemViewModel MapToCustomerItemViewModel(SupportTicket t, bool includeMessages = false)
        {
            var vm = new SupportTicketCustomerItemViewModel
            {
                Id = t.Id,
                Code = t.Code,
                Subject = t.Subject,
                Category = t.Category,
                CategoryLabel = GetCategoryLabel(t.Category),
                Status = t.Status,
                Priority = t.Priority,
                LastMessagePreview = t.Messages?.OrderByDescending(m => m.CreatedAt).FirstOrDefault()?.Content ?? "",
                UnreadCount = t.UnreadCount,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                RelatedOrderId = t.RelatedOrderId,
                RelatedProductId = t.RelatedProductId
            };

            if (includeMessages && t.Messages != null)
            {
                vm.Messages = t.Messages.OrderBy(m => m.CreatedAt).Select(m => new SupportTicketCustomerMessageViewModel
                {
                    Id = m.Id,
                    SenderName = m.SenderName,
                    SenderType = m.SenderType,
                    SenderRole = m.SenderType == "customer" ? "Khách hàng" : (m.SenderType == "system" ? "Hệ thống" : "Admin"),
                    Content = m.Content ?? "",
                    StickerCode = m.StickerCode,
                    CreatedAt = m.CreatedAt,
                    Attachments = !string.IsNullOrEmpty(m.Attachments) ? System.Text.Json.JsonSerializer.Deserialize<List<SupportTicketAttachmentViewModel>>(m.Attachments, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<SupportTicketAttachmentViewModel>() : new List<SupportTicketAttachmentViewModel>()
                }).ToList();
            }

            return vm;
        }
    }
}
