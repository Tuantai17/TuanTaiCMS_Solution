using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using CMS.Data.Enums;

namespace CMS.Backend.Services
{
    public class OrderIssueService : IOrderIssueService
    {
        private readonly ApplicationDbContext _context;

        public OrderIssueService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckCanReportIssueAsync(int orderId, int orderDetailId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.OrderDetails == null) return false;

            // Only allow reporting issues in PENDING, CONFIRMED, or PROCESSING state
            if (order.Status != (int)OrderStatus.PENDING && 
                order.Status != (int)OrderStatus.CONFIRMED && 
                order.Status != (int)OrderStatus.PROCESSING)
            {
                return false;
            }

            var detail = order.OrderDetails.FirstOrDefault(d => d.Id == orderDetailId);
            if (detail == null) return false;

            // Check if already has an open issue
            var existingIssue = await _context.OrderItemIssues
                .AnyAsync(i => i.OrderDetailId == orderDetailId && 
                               (i.Status == OrderItemIssueStatus.Open || i.Status == OrderItemIssueStatus.WaitingForCustomer || i.Status == OrderItemIssueStatus.WaitingForRestock));
            
            return !existingIssue;
        }

        public async Task<CMS.Backend.Models.ReportOrderItemIssueViewModel> GetReportIssueDataAsync(int orderId, int orderDetailId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) throw new Exception("Không tìm thấy đơn hàng.");

            var detail = order.OrderDetails!.FirstOrDefault(d => d.Id == orderDetailId);
            if (detail == null) throw new Exception("Không tìm thấy chi tiết sản phẩm trong đơn.");
            
            if (detail.Product == null) throw new Exception("Không tìm thấy sản phẩm.");

            // Calculate values
            int orderedQuantity = detail.OriginalQuantity > 0 ? detail.OriginalQuantity : detail.Quantity;
            int reservedQuantity = orderedQuantity; // This should be calculated based on inventory logic, simplified here.
            int physicalQuantity = detail.Product.StockQuantity;
            
            // Fulfillable could be physical if we don't have enough, otherwise reserved.
            int fulfillableQuantity = Math.Min(orderedQuantity, physicalQuantity);

            var model = new CMS.Backend.Models.ReportOrderItemIssueViewModel
            {
                OrderId = orderId,
                OrderDetailId = orderDetailId,
                ProductName = detail.Product.Name,
                SKU = $"PROD-{detail.Product.Id}", // Fallback for SKU since it's not in the model
                ProductImageUrl = detail.Product.ImageUrl,
                OrderedQuantity = orderedQuantity,
                ReservedQuantity = reservedQuantity,
                PhysicalQuantity = physicalQuantity,
                FulfillableQuantity = fulfillableQuantity,
                DamagedQuantity = 0,
                MissingQuantity = 0,
                IssueType = null
            };

            return model;
        }

        public async Task<OrderItemIssue> ReportItemIssueAsync(CMS.Backend.Models.ReportOrderItemIssueViewModel request, string performedBy)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order == null) throw new Exception("Không tìm thấy đơn hàng.");

                var detail = order.OrderDetails!.FirstOrDefault(d => d.Id == request.OrderDetailId);
                if (detail == null) throw new Exception("Không tìm thấy chi tiết sản phẩm trong đơn.");

                // Create the issue
                var issue = new OrderItemIssue
                {
                    OrderId = request.OrderId,
                    OrderDetailId = request.OrderDetailId,
                    ProductId = detail.ProductId,
                    IssueType = request.IssueType!.Value,
                    OrderedQuantity = detail.Quantity,
                    FulfillableQuantity = request.FulfillableQuantity,
                    DamagedQuantity = request.DamagedQuantity,
                    MissingQuantity = request.MissingQuantity,
                    Reason = request.Reason,
                    InternalNote = request.InternalNote,
                    Status = OrderItemIssueStatus.WaitingForCustomer,
                    ReportedBy = performedBy,
                    ReportedAt = DateTime.Now
                };

                _context.OrderItemIssues.Add(issue);

                // Update OrderDetail
                detail.OriginalQuantity = detail.Quantity;
                detail.FulfillableQuantity = request.FulfillableQuantity;
                detail.DamagedQuantity = request.DamagedQuantity;
                detail.MissingQuantity = request.MissingQuantity;
                detail.ItemStatus = OrderItemStatus.AwaitingCustomer;
                detail.IssueType = request.IssueType.ToString();
                detail.IssueReason = request.Reason;
                detail.InternalNote = request.InternalNote;
                detail.IssueReportedAt = DateTime.Now;
                detail.IssueReportedBy = performedBy;

                // Update Order status to AwaitingCustomerConfirmation
                order.Status = (int)OrderStatus.AWAITING_CUSTOMER_CONFIRMATION;

                await _context.SaveChangesAsync();

                // Add activity log
                var product = await _context.Products.FindAsync(detail.ProductId);
                var productName = product?.Name ?? $"Sản phẩm {detail.ProductId}";
                await AddOrderActivityAsync(order.Id, "Báo cáo sự cố", $"Phát hiện sự cố ({productName}): {request.Reason}. Đơn hàng chuyển sang chờ khách xác nhận.", performedBy);

                await transaction.CommitAsync();
                return issue;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CMS.Backend.Models.ResolveOrderItemIssueViewModel> GetResolutionDataAsync(int issueId)
        {
            var issue = await _context.OrderItemIssues
                .Include(i => i.Order)
                .ThenInclude(o => o.OrderDetails)
                .FirstOrDefaultAsync(i => i.Id == issueId);

            if (issue == null || issue.Order == null)
                throw new Exception("Không tìm thấy sự cố này.");

            var detail = issue.Order.OrderDetails!.FirstOrDefault(d => d.Id == issue.OrderDetailId);
            if (detail == null)
                throw new Exception("Không tìm thấy thông tin sản phẩm trong đơn hàng.");

            var product = await _context.Products.FindAsync(detail.ProductId);

            return new CMS.Backend.Models.ResolveOrderItemIssueViewModel
            {
                IssueId = issue.Id,
                OrderId = issue.OrderId,
                OrderDetailId = issue.OrderDetailId,
                ProductName = product?.Name ?? $"Sản phẩm {detail.ProductId}",
                SKU = product != null ? $"PROD-{product.Id}" : "N/A",
                ProductImageUrl = product?.ImageUrl,
                OrderedQuantity = issue.OrderedQuantity,
                FulfillableQuantity = issue.FulfillableQuantity,
                DamagedQuantity = issue.DamagedQuantity,
                MissingQuantity = issue.MissingQuantity,
                IssueReason = issue.Reason ?? "Không rõ lý do",
                Decision = CustomerIssueDecision.AcceptReducedQuantity
            };
        }

        public async Task<OrderIssueResolutionPreviewViewModel> GetIssueResolutionPreviewAsync(int issueId, CustomerIssueDecision decision, int? adjustedQuantity)
        {
            var issue = await _context.OrderItemIssues
                .Include(i => i.Order)
                .ThenInclude(o => o.OrderDetails)
                .FirstOrDefaultAsync(i => i.Id == issueId);

            if (issue == null || issue.Order == null) throw new Exception("Không tìm thấy sự cố.");

            var orderDetails = issue.Order.OrderDetails!;
            decimal oldSubtotal = orderDetails.Sum(d => d.Quantity * d.UnitPrice);
            decimal newSubtotal = oldSubtotal;
            
            var targetDetail = orderDetails.FirstOrDefault(d => d.Id == issue.OrderDetailId);
            if (targetDetail == null) throw new Exception("Không tìm thấy OrderDetail.");

            int oldQuantity = targetDetail.Quantity;
            int newQuantity = oldQuantity;

            if (decision == CustomerIssueDecision.AcceptReducedQuantity && adjustedQuantity.HasValue)
            {
                newQuantity = adjustedQuantity.Value;
                decimal oldLineTotal = oldQuantity * targetDetail.UnitPrice;
                decimal newLineTotal = newQuantity * targetDetail.UnitPrice;
                newSubtotal = oldSubtotal - oldLineTotal + newLineTotal;
            }
            else if (decision == CustomerIssueDecision.RemoveItem)
            {
                newQuantity = 0;
                decimal oldLineTotal = oldQuantity * targetDetail.UnitPrice;
                newSubtotal = oldSubtotal - oldLineTotal;
            }
            else if (decision == CustomerIssueDecision.CancelEntireOrder)
            {
                newSubtotal = 0;
            }

            return new OrderIssueResolutionPreviewViewModel
            {
                OldSubtotal = oldSubtotal,
                NewSubtotal = newSubtotal,
                OldTotal = oldSubtotal, // Assuming no shipping fee logic for simplicity, adjust if needed
                NewTotal = newSubtotal,
                RefundAmount = oldSubtotal - newSubtotal,
                OldQuantity = oldQuantity,
                NewQuantity = newQuantity
            };
        }

        public async Task<bool> ResolveIssueAsync(CMS.Backend.Models.ResolveOrderItemIssueViewModel request, string performedBy)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var issue = await _context.OrderItemIssues
                    .Include(i => i.Order)
                    .ThenInclude(o => o.OrderDetails)
                    .FirstOrDefaultAsync(i => i.Id == request.IssueId);

                if (issue == null || issue.Order == null) throw new Exception("Không tìm thấy sự cố.");

                if (issue.Status == OrderItemIssueStatus.Resolved || issue.Status == OrderItemIssueStatus.OrderCancelled || issue.Status == OrderItemIssueStatus.ItemRemoved || issue.Status == OrderItemIssueStatus.CustomerAcceptedAdjustment)
                    throw new Exception("Sự cố này đã được xử lý trước đó.");

                var order = issue.Order;
                var detail = order.OrderDetails!.FirstOrDefault(d => d.Id == issue.OrderDetailId);
                if (detail == null) throw new Exception("Không tìm thấy OrderDetail.");

                issue.CustomerDecision = request.Decision.ToString();
                issue.CustomerNote = request.CustomerNote;
                issue.ResolvedBy = performedBy;
                issue.ResolvedAt = DateTime.Now;

                detail.CustomerDecision = request.Decision.ToString();
                detail.CustomerConfirmedAt = DateTime.Now;
                detail.CustomerConfirmedBy = performedBy;

                decimal oldSubtotal = order.OrderDetails!.Sum(d => d.Quantity * d.UnitPrice);

                var product = await _context.Products.FindAsync(detail.ProductId);
                var productName = product?.Name ?? $"Sản phẩm {detail.ProductId}";

                switch (request.Decision)
                {
                    case CustomerIssueDecision.AcceptReducedQuantity:
                        if (!request.AdjustedQuantity.HasValue || request.AdjustedQuantity.Value <= 0) 
                            throw new Exception("Vui lòng nhập số lượng điều chỉnh hợp lệ.");
                        if (request.AdjustedQuantity.Value > issue.FulfillableQuantity)
                            throw new Exception("Số lượng điều chỉnh không được lớn hơn số lượng có thể giao.");
                        
                        detail.AdjustedQuantity = request.AdjustedQuantity.Value;
                        detail.Quantity = request.AdjustedQuantity.Value;
                        detail.ItemStatus = OrderItemStatus.QuantityAdjusted;
                        issue.Status = OrderItemIssueStatus.CustomerAcceptedAdjustment;
                        
                        if (issue.DamagedQuantity > 0)
                        {
                            await MarkDamagedInventoryAsync(detail.ProductId, issue.DamagedQuantity, order.Id, "Hư hỏng khi chuẩn bị", performedBy);
                        }
                        
                        await AddOrderActivityAsync(order.Id, "Ghi nhận phương án", $"Khách đồng ý nhận {request.AdjustedQuantity.Value} sản phẩm ({productName}) (qua {request.ContactMethod}). Ghi chú: {request.CustomerNote}", performedBy);
                        break;

                    case CustomerIssueDecision.RemoveItem:
                        detail.AdjustedQuantity = 0;
                        detail.Quantity = 0;
                        detail.ItemStatus = OrderItemStatus.Removed;
                        issue.Status = OrderItemIssueStatus.ItemRemoved;
                        
                        if (issue.DamagedQuantity > 0)
                        {
                            await MarkDamagedInventoryAsync(detail.ProductId, issue.DamagedQuantity, order.Id, "Hư hỏng khi chuẩn bị", performedBy);
                        }
                        
                        await AddOrderActivityAsync(order.Id, "Ghi nhận phương án", $"Khách yêu cầu loại sản phẩm ({productName}) khỏi đơn (qua {request.ContactMethod}). Ghi chú: {request.CustomerNote}", performedBy);
                        break;

                    case CustomerIssueDecision.WaitForRestock:
                        detail.ItemStatus = OrderItemStatus.AwaitingCustomer;
                        issue.Status = OrderItemIssueStatus.WaitingForRestock;
                        order.Status = (int)OrderStatus.WAITING_FOR_RESTOCK;
                        
                        string dateNote = request.ExpectedRestockDate.HasValue ? $" Ngày dự kiến: {request.ExpectedRestockDate.Value.ToString("dd/MM/yyyy")}." : "";
                        await AddOrderActivityAsync(order.Id, "Ghi nhận phương án", $"Khách đồng ý chờ bổ sung hàng ({productName}) (qua {request.ContactMethod}).{dateNote} Ghi chú: {request.CustomerNote}", performedBy);
                        break;

                    case CustomerIssueDecision.CancelEntireOrder:
                        detail.ItemStatus = OrderItemStatus.Cancelled;
                        issue.Status = OrderItemIssueStatus.OrderCancelled;
                        
                        bool isPaidOnline = order.PaymentMethod != "COD" && order.PaymentStatus == 1;
                        if (isPaidOnline)
                        {
                            order.Status = (int)OrderStatus.REFUND_PENDING; // Chờ hoàn tiền
                            await AddOrderActivityAsync(order.Id, "Yêu cầu hoàn tiền", "Đơn hàng đã thanh toán online, chuyển sang chờ hoàn tiền.", performedBy);
                        }
                        else
                        {
                            order.Status = (int)OrderStatus.CANCELLED;
                        }
                        
                        if (issue.DamagedQuantity > 0)
                        {
                            await MarkDamagedInventoryAsync(detail.ProductId, issue.DamagedQuantity, order.Id, "Hư hỏng khi chuẩn bị", performedBy);
                        }
                        
                        await AddOrderActivityAsync(order.Id, "Ghi nhận phương án", $"Khách yêu cầu hủy toàn bộ đơn hàng do sự cố của ({productName}) (qua {request.ContactMethod}). Ghi chú: {request.CustomerNote}", performedBy);
                        break;
                }

                await _context.SaveChangesAsync();
                
                if (request.Decision == CustomerIssueDecision.AcceptReducedQuantity || request.Decision == CustomerIssueDecision.RemoveItem)
                {
                    await RecalculateOrderTotalsAsync(order.Id);
                    
                    var hasOtherIssues = await _context.OrderItemIssues
                        .AnyAsync(i => i.OrderId == order.Id && i.Id != issue.Id && 
                                       (i.Status == OrderItemIssueStatus.Open || i.Status == OrderItemIssueStatus.WaitingForCustomer));
                    
                    if (!hasOtherIssues)
                    {
                        var remainingValidItems = order.OrderDetails!.Any(d => d.Quantity > 0);
                        if (!remainingValidItems)
                        {
                            order.Status = (int)OrderStatus.CANCELLED;
                            await AddOrderActivityAsync(order.Id, "Hủy đơn hàng tự động", "Đơn hàng bị hủy do không còn sản phẩm nào hợp lệ.", performedBy);
                        }
                        else
                        {
                            order.Status = (int)OrderStatus.PROCESSING;
                            await AddOrderActivityAsync(order.Id, "Tiếp tục chuẩn bị", $"Đã giải quyết xong sự cố ({productName}), đơn hàng tiếp tục được chuẩn bị.", performedBy);
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                decimal newSubtotal = order.OrderDetails!.Sum(d => d.Quantity * d.UnitPrice);
                if (newSubtotal != oldSubtotal)
                {
                     await AddOrderActivityAsync(order.Id, "Cập nhật tổng tiền", $"Tổng tiền thay đổi từ {oldSubtotal:N0}đ xuống {newSubtotal:N0}đ.", performedBy);
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RecalculateOrderTotalsAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null && order.OrderDetails != null)
            {
                // In a real scenario, you'd update Order.TotalAmount here if it exists.
                // Assuming Order entity doesn't have a TotalAmount property explicitly stored based on Order.cs provided.
                // It calculates on the fly or in another way. If there is a TotalAmount, we update it.
            }
        }

        public async Task MarkDamagedInventoryAsync(int productId, int quantity, int orderId, string reason, string performedBy)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (inventory != null)
            {
                int stockBefore = inventory.CurrentStock;
                int reservedBefore = inventory.ReservedStock;
                
                inventory.CurrentStock -= quantity;
                inventory.ReservedStock -= quantity;
                inventory.UpdatedAt = DateTime.Now;

                var trans = new InventoryTransaction
                {
                    InventoryId = inventory.Id,
                    TransactionType = "ADJUSTMENT_OUT",
                    QuantityChange = -quantity,
                    StockBefore = stockBefore,
                    StockAfter = inventory.CurrentStock,
                    ReservedBefore = reservedBefore,
                    ReservedAfter = inventory.ReservedStock,
                    ReferenceId = orderId,
                    ReferenceType = "OrderIssue",
                    Reason = reason,
                    CreatedBy = performedBy,
                    CreatedAt = DateTime.Now
                };
                _context.InventoryTransactions.Add(trans);
            }
        }

        public async Task ReleaseReservedInventoryAsync(int productId, int quantity, int orderId, string reason)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (inventory != null)
            {
                int stockBefore = inventory.CurrentStock;
                int reservedBefore = inventory.ReservedStock;
                
                inventory.ReservedStock -= quantity;
                inventory.UpdatedAt = DateTime.Now;

                var trans = new InventoryTransaction
                {
                    InventoryId = inventory.Id,
                    TransactionType = "RELEASE",
                    QuantityChange = 0, // CurrentStock doesn't change
                    StockBefore = stockBefore,
                    StockAfter = inventory.CurrentStock,
                    ReservedBefore = reservedBefore,
                    ReservedAfter = inventory.ReservedStock,
                    ReferenceId = orderId,
                    ReferenceType = "OrderIssue",
                    Reason = reason,
                    CreatedBy = "System",
                    CreatedAt = DateTime.Now
                };
                _context.InventoryTransactions.Add(trans);
            }
        }

        public async Task AddOrderActivityAsync(int orderId, string actionType, string description, string performedBy)
        {
            var log = new OrderActivityLog
            {
                OrderId = orderId,
                ActionType = actionType,
                Description = description,
                PerformedBy = performedBy,
                CreatedAt = DateTime.Now
            };
            _context.OrderActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<OrderItemIssue>> GetOrderIssuesAsync(int orderId)
        {
            return await _context.OrderItemIssues
                .Include(i => i.Product)
                .Where(i => i.OrderId == orderId)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }
    }
}
