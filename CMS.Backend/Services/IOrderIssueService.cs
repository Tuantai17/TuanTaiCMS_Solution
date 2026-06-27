using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CMS.Data.Entities;
using CMS.Data.Enums;

namespace CMS.Backend.Services
{
    // DTO cho thao tac tao Issue (cũ - được thay thế bởi ReportOrderItemIssueViewModel ở Controller, nhưng có thể giữ lại đây nếu cần mapping)
    public class ReportOrderItemIssueRequest
    {
        public int OrderId { get; set; }
        public int OrderDetailId { get; set; }
        public OrderItemIssueType IssueType { get; set; }
        public int FulfillableQuantity { get; set; }
        public int DamagedQuantity { get; set; }
        public int MissingQuantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? InternalNote { get; set; }
    }

    // DTO cho thao tac giai quyet
    public class ResolveOrderItemIssueRequest
    {
        public int IssueId { get; set; }
        public CustomerIssueDecision Decision { get; set; }
        public int? AdjustedQuantity { get; set; }
        public DateTime? ExpectedRestockDate { get; set; }
        public string? CustomerNote { get; set; }
        public string? ContactMethod { get; set; }
    }

    public class OrderIssueResolutionPreviewViewModel
    {
        public decimal OldSubtotal { get; set; }
        public decimal NewSubtotal { get; set; }
        public decimal OldTotal { get; set; }
        public decimal NewTotal { get; set; }
        public decimal RefundAmount { get; set; }
        public int OldQuantity { get; set; }
        public int NewQuantity { get; set; }
    }

    public interface IOrderIssueService
    {
        Task<bool> CheckCanReportIssueAsync(int orderId, int orderDetailId);
        
        Task<CMS.Backend.Models.ReportOrderItemIssueViewModel> GetReportIssueDataAsync(int orderId, int orderDetailId);
        
        Task<OrderItemIssue> ReportItemIssueAsync(CMS.Backend.Models.ReportOrderItemIssueViewModel request, string performedBy);
        
        Task<CMS.Backend.Models.ResolveOrderItemIssueViewModel> GetResolutionDataAsync(int issueId);

        Task<OrderIssueResolutionPreviewViewModel> GetIssueResolutionPreviewAsync(int issueId, CustomerIssueDecision decision, int? adjustedQuantity);
        
        Task<bool> ResolveIssueAsync(CMS.Backend.Models.ResolveOrderItemIssueViewModel request, string performedBy);
        
        Task RecalculateOrderTotalsAsync(int orderId);
        
        Task MarkDamagedInventoryAsync(int productId, int quantity, int orderId, string reason, string performedBy);
        
        Task ReleaseReservedInventoryAsync(int productId, int quantity, int orderId, string reason);
        
        Task AddOrderActivityAsync(int orderId, string actionType, string description, string performedBy);
        
        Task<List<OrderItemIssue>> GetOrderIssuesAsync(int orderId);
    }
}
