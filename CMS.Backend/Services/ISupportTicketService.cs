using CMS.Backend.Models;
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Services
{
    public interface ISupportTicketService
    {
        Task<SupportTicketAdminListPageViewModel> GetAdminTicketsAsync(SupportTicketAdminFilterViewModel filter);
        Task<SupportTicketAdminDetailPageViewModel?> GetAdminTicketDetailAsync(string id, bool markAsRead = false);
        Task<SupportTicketMessage> SendAdminReplyAsync(string ticketId, string content, string staffName);
        Task UpdateAdminTicketStatusAsync(string ticketId, string status);
        
        // API methods for frontend
        Task<SupportTicketCustomerPagedResult> GetCustomerTicketsAsync(int customerId, SupportTicketCustomerFilter filter);
        Task<int> GetCustomerUnreadTicketCountAsync(int customerId);
        Task<SupportTicketCustomerItemViewModel?> GetCustomerTicketDetailAsync(string ticketId, int customerId, bool markAsRead = false);
        Task<SupportTicketCustomerItemViewModel> CreateTicketAsync(CreateSupportTicketRequest request, int customerId);
        Task<SupportTicketCustomerItemViewModel> AddCustomerMessageAsync(string ticketId, CreateSupportTicketMessageRequest request, int customerId);
        Task<SupportTicketCustomerItemViewModel> ReopenTicketAsync(string ticketId, int customerId);
    }
}
