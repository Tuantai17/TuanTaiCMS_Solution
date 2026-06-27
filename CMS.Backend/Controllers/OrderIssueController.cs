using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMS.Backend.Services;
using CMS.Data.Enums;
using System.Linq;

namespace CMS.Backend.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    [Route("Admin/OrderIssue")]
    public class OrderIssueController : Controller
    {
        private readonly IOrderIssueService _orderIssueService;

        public OrderIssueController(IOrderIssueService orderIssueService)
        {
            _orderIssueService = orderIssueService;
        }

        [HttpGet("GetReportItemIssueModal")]
        public async Task<IActionResult> GetReportItemIssueModal(int orderId, int orderDetailId)
        {
            try
            {
                var model = await _orderIssueService.GetReportIssueDataAsync(orderId, orderDetailId);
                return PartialView("~/Views/Order/_ReportItemIssueModal.cshtml", model);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Report")]
        public async Task<IActionResult> ReportIssue([FromBody] CMS.Backend.Models.ReportOrderItemIssueViewModel request)
        {
            try
            {
                var username = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
                
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return BadRequest(new { success = false, message = errors });
                }

                var canReport = await _orderIssueService.CheckCanReportIssueAsync(request.OrderId, request.OrderDetailId);
                if (!canReport)
                {
                    return BadRequest(new { success = false, message = "Không thể báo cáo sự cố cho sản phẩm này ở trạng thái hiện tại hoặc đã có sự cố đang xử lý." });
                }

                var issue = await _orderIssueService.ReportItemIssueAsync(request, username);
                return Ok(new { success = true, message = "Đã ghi nhận sự cố thành công. Đơn hàng chuyển sang trạng thái chờ khách xác nhận.", issueId = issue.Id });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetResolveIssueModal")]
        public async Task<IActionResult> GetResolveIssueModal(int issueId)
        {
            try
            {
                var model = await _orderIssueService.GetResolutionDataAsync(issueId);
                return PartialView("~/Views/Order/_ResolveOrderItemIssueModal.cshtml", model);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("PreviewResolution")]
        public async Task<IActionResult> PreviewResolution(int issueId, CustomerIssueDecision decision, int? adjustedQuantity)
        {
            try
            {
                var preview = await _orderIssueService.GetIssueResolutionPreviewAsync(issueId, decision, adjustedQuantity);
                return Ok(new { success = true, preview });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Resolve")]
        public async Task<IActionResult> ResolveIssue([FromBody] CMS.Backend.Models.ResolveOrderItemIssueViewModel request)
        {
            try
            {
                var username = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
                var result = await _orderIssueService.ResolveIssueAsync(request, username);
                
                return Ok(new { success = true, message = "Đã ghi nhận phương án xử lý thành công." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        
        [HttpGet("List/{orderId}")]
        public async Task<IActionResult> GetIssues(int orderId)
        {
            var issues = await _orderIssueService.GetOrderIssuesAsync(orderId);
            return Ok(new { success = true, data = issues });
        }
    }
}
