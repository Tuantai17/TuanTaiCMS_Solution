/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 19/6/2026
Mô tả: ViewModel tổng hợp dữ liệu cho trang Dashboard quản trị bán hàng thương mại điện tử.
*/

namespace CMS.Backend.Models
{
    /// <summary>
    /// ViewModel chính cho trang Dashboard Admin, chứa toàn bộ dữ liệu thống kê từ database.
    /// </summary>
    public class DashboardViewModel
    {
        // === THỐNG KÊ TỔNG QUAN (4 Stat Cards) ===
        public decimal TotalRevenue { get; set; }         // Tổng doanh thu (từ đơn hoàn thành Status=2)
        public string RevenueFilter { get; set; } = "all"; // Bộ lọc doanh thu
        public DateTime? StartDate { get; set; }          // Từ ngày (nếu chọn custom)
        public DateTime? EndDate { get; set; }            // Đến ngày (nếu chọn custom)

        public int TotalOrders { get; set; }              // Tổng số đơn hàng
        public int TotalProducts { get; set; }            // Tổng số sản phẩm
        public int TotalCustomers { get; set; }           // Tổng số khách hàng

        // === THỐNG KÊ ĐƠN HÀNG THEO TRẠNG THÁI ===
        public int PendingOrders { get; set; }            // Đơn chờ duyệt (Status=0)
        public int ShippingOrders { get; set; }           // Đơn đang giao (Status=1)
        public int CompletedOrders { get; set; }          // Đơn hoàn thành (Status=2)
        public int CancelledOrders { get; set; }          // Đơn đã hủy (Status=3)

        // === BIỂU ĐỒ DOANH THU ===
        public List<DailyRevenueDto> DailyRevenues { get; set; } = new();
        public string ChartTitle { get; set; } = "Doanh thu 7 ngày gần nhất";
        public string ChartSubtitle { get; set; } = "Thống kê doanh thu từ đơn hàng hoàn thành";

        // === TOP 5 SẢN PHẨM BÁN CHẠY ===
        public List<TopProductDto> TopProducts { get; set; } = new();

        // === 5 ĐƠN HÀNG MỚI NHẤT ===
        public List<RecentOrderDto> RecentOrders { get; set; } = new();

        // === THỐNG KÊ BÀI VIẾT & DANH MỤC ===
        public int TotalPosts { get; set; }
        public int TotalCategories { get; set; }
    }

    /// <summary>
    /// DTO doanh thu theo ngày, phục vụ biểu đồ Chart.js.
    /// </summary>
    public class DailyRevenueDto
    {
        public string Date { get; set; } = string.Empty;   // Ngày (dd/MM)
        public decimal Revenue { get; set; }                // Doanh thu trong ngày
        public int OrderCount { get; set; }                 // Số đơn hàng trong ngày
    }

    /// <summary>
    /// DTO sản phẩm bán chạy nhất, phục vụ bảng xếp hạng Top Products.
    /// </summary>
    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalQuantitySold { get; set; }          // Tổng số lượng đã bán
        public decimal TotalRevenue { get; set; }           // Tổng doanh thu sản phẩm
    }

    /// <summary>
    /// DTO đơn hàng gần nhất, phục vụ bảng Recent Orders.
    /// </summary>
    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public decimal TotalAmount { get; set; }            // Tổng tiền đơn hàng
        public int Status { get; set; }                     // 0: Chờ duyệt, 1: Đang giao, 2: Hoàn thành, 3: Đã hủy
        public int ItemCount { get; set; }                  // Số sản phẩm trong đơn
    }
}
