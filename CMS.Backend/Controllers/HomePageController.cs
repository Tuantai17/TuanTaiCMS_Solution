/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 15/5/2026
Cập nhật: 19/6/2026
Mô tả: Controller trang chủ quản trị (Dashboard). Truy vấn dữ liệu thật từ database
        để hiển thị thống kê tổng quan, biểu đồ doanh thu, top sản phẩm bán chạy và đơn hàng mới nhất.
*/

using CMS.Backend.Models;
using CMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    // HomeController xử lý request MVC cho trang Dashboard quản trị (/Home/Index).
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor nhận ApplicationDbContext từ Dependency Injection.
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action Index: Hiển thị Dashboard với dữ liệu thống kê thật từ database.
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index(string revenueFilter = "all", DateTime? startDate = null, DateTime? endDate = null)
        {
            // === 1. THỐNG KÊ TỔNG QUAN ===
            var totalOrders = await _context.Orders.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var totalPosts = await _context.Posts.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();

            // Tổng doanh thu tính từ đơn hàng đã hoàn thành (Status = 4)
            var revenueQuery = _context.OrderDetails
                .Where(od => od.Order != null && od.Order.Status == 4);

            var now = DateTime.Now;
            if (revenueFilter == "24h")
            {
                var yesterday = now.AddHours(-24);
                revenueQuery = revenueQuery.Where(od => od.Order.OrderDate >= yesterday);
            }
            else if (revenueFilter == "7days")
            {
                var filterSevenDaysAgo = now.Date.AddDays(-7);
                revenueQuery = revenueQuery.Where(od => od.Order.OrderDate >= filterSevenDaysAgo);
            }
            else if (revenueFilter == "30days")
            {
                var thirtyDaysAgo = now.Date.AddDays(-30);
                revenueQuery = revenueQuery.Where(od => od.Order.OrderDate >= thirtyDaysAgo);
            }
            else if (revenueFilter == "custom" && startDate.HasValue && endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1); // Cuối ngày
                revenueQuery = revenueQuery.Where(od => od.Order.OrderDate >= startDate.Value.Date && od.Order.OrderDate <= end);
            }

            var totalRevenue = await revenueQuery.SumAsync(od => (decimal?)(od.Quantity * od.UnitPrice)) ?? 0;

            // === 2. THỐNG KÊ ĐƠN HÀNG THEO TRẠNG THÁI ===
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == 0); // Chờ duyệt
            var shippingOrders = await _context.Orders.CountAsync(o => o.Status == 3); // Đang giao hàng
            var completedOrders = await _context.Orders.CountAsync(o => o.Status == 4); // Hoàn thành
            var cancelledOrders = await _context.Orders.CountAsync(o => o.Status == 5); // Đã hủy

            // === 3. BIỂU ĐỒ DOANH THU ===
            var chartTitle = "Doanh thu 7 ngày gần nhất";
            var chartSubtitle = "Thống kê doanh thu theo ngày";
            var dailyRevenues = new List<DailyRevenueDto>();

            var baseChartQuery = _context.Orders.Where(o => o.Status == 4);

            if (revenueFilter == "24h")
            {
                chartTitle = "Doanh thu 24 giờ qua";
                chartSubtitle = "Thống kê doanh thu theo giờ";
                var yesterdayHours = DateTime.Now.AddHours(-23);
                
                var rawData = await baseChartQuery
                    .Where(o => o.OrderDate >= yesterdayHours)
                    .Join(_context.OrderDetails, o => o.Id, od => od.OrderId, (o, od) => new { o.OrderDate, od.Quantity, od.UnitPrice })
                    .ToListAsync();
                    
                var groupedData = rawData
                    .GroupBy(x => new { x.OrderDate.Date, x.OrderDate.Hour })
                    .Select(g => new { 
                        DateHour = new DateTime(g.Key.Date.Year, g.Key.Date.Month, g.Key.Date.Day, g.Key.Hour, 0, 0),
                        Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                    }).ToList();
                    
                for (int i = 0; i < 24; i++)
                {
                    var hourDt = yesterdayHours.AddHours(i);
                    var match = groupedData.FirstOrDefault(r => r.DateHour.Date == hourDt.Date && r.DateHour.Hour == hourDt.Hour);
                    dailyRevenues.Add(new DailyRevenueDto { Date = hourDt.ToString("HH:00"), Revenue = match?.Revenue ?? 0 });
                }
            }
            else if (revenueFilter == "30days")
            {
                chartTitle = "Doanh thu 30 ngày qua";
                chartSubtitle = "Thống kê doanh thu theo ngày";
                var thirtyDaysAgo = DateTime.Today.AddDays(-29);
                
                var rawData = await baseChartQuery
                    .Where(o => o.OrderDate.Date >= thirtyDaysAgo)
                    .Join(_context.OrderDetails, o => o.Id, od => od.OrderId, (o, od) => new { o.OrderDate, od.Quantity, od.UnitPrice })
                    .ToListAsync();
                    
                var groupedData = rawData.GroupBy(x => x.OrderDate.Date)
                    .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.Quantity * x.UnitPrice) }).ToList();
                    
                for (int i = 0; i < 30; i++)
                {
                    var date = thirtyDaysAgo.AddDays(i);
                    var match = groupedData.FirstOrDefault(r => r.Date == date);
                    dailyRevenues.Add(new DailyRevenueDto { Date = date.ToString("dd/MM"), Revenue = match?.Revenue ?? 0 });
                }
            }
            else if (revenueFilter == "custom" && startDate.HasValue && endDate.HasValue)
            {
                chartTitle = $"Doanh thu từ {startDate.Value:dd/MM/yyyy} đến {endDate.Value:dd/MM/yyyy}";
                chartSubtitle = "Thống kê doanh thu theo ngày";
                
                var customStart = startDate.Value.Date;
                var customEnd = endDate.Value.Date;
                var endTick = customEnd.AddDays(1).AddTicks(-1);
                
                var rawData = await baseChartQuery
                    .Where(o => o.OrderDate >= customStart && o.OrderDate <= endTick)
                    .Join(_context.OrderDetails, o => o.Id, od => od.OrderId, (o, od) => new { o.OrderDate, od.Quantity, od.UnitPrice })
                    .ToListAsync();
                    
                var groupedData = rawData.GroupBy(x => x.OrderDate.Date)
                    .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.Quantity * x.UnitPrice) }).ToList();
                    
                int days = (int)(customEnd - customStart).TotalDays + 1;
                if(days > 90) days = 90; // Limit max days for chart to avoid memory issues
                
                for (int i = 0; i < days; i++)
                {
                    var date = customStart.AddDays(i);
                    var match = groupedData.FirstOrDefault(r => r.Date == date);
                    dailyRevenues.Add(new DailyRevenueDto { Date = date.ToString("dd/MM"), Revenue = match?.Revenue ?? 0 });
                }
            }
            else if (revenueFilter == "all")
            {
                chartTitle = "Tổng doanh thu toàn thời gian";
                chartSubtitle = "Thống kê doanh thu theo tháng";
                
                var rawData = await baseChartQuery
                    .Join(_context.OrderDetails, o => o.Id, od => od.OrderId, (o, od) => new { o.OrderDate, od.Quantity, od.UnitPrice })
                    .ToListAsync();
                    
                var groupedData = rawData.GroupBy(x => new { x.OrderDate.Year, x.OrderDate.Month })
                    .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Revenue = g.Sum(x => x.Quantity * x.UnitPrice) })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();
                    
                if (!groupedData.Any())
                {
                     dailyRevenues.Add(new DailyRevenueDto { Date = DateTime.Now.ToString("MM/yyyy"), Revenue = 0 });
                }
                else
                {
                    foreach (var item in groupedData)
                    {
                        dailyRevenues.Add(new DailyRevenueDto { Date = $"{item.Month:D2}/{item.Year}", Revenue = item.Revenue });
                    }
                }
            }
            else
            {
                // Mặc định là 7days
                chartTitle = "Doanh thu 7 ngày gần nhất";
                chartSubtitle = "Thống kê doanh thu theo ngày";
                var today = DateTime.Today;
                var sevenDaysAgo = today.AddDays(-6);
                
                var rawData = await baseChartQuery
                    .Where(o => o.OrderDate.Date >= sevenDaysAgo && o.OrderDate.Date <= today)
                    .Join(_context.OrderDetails, o => o.Id, od => od.OrderId, (o, od) => new { o.OrderDate, od.Quantity, od.UnitPrice })
                    .ToListAsync();
                    
                var groupedData = rawData.GroupBy(x => x.OrderDate.Date)
                    .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.Quantity * x.UnitPrice) }).ToList();
                    
                for (int i = 0; i < 7; i++)
                {
                    var date = sevenDaysAgo.AddDays(i);
                    var match = groupedData.FirstOrDefault(r => r.Date == date);
                    dailyRevenues.Add(new DailyRevenueDto { Date = date.ToString("dd/MM"), Revenue = match?.Revenue ?? 0 });
                }
            }

            // === 4. TOP 5 SẢN PHẨM BÁN CHẠY (tính trên tất cả đơn hàng, trừ đơn hủy) ===
            var topProducts = await _context.OrderDetails
                .Where(od => od.Order != null && od.Order.Status != 5)
                .GroupBy(od => od.ProductId)
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key,
                    TotalQuantitySold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .OrderByDescending(p => p.TotalQuantitySold)
                .Take(5)
                .ToListAsync();

            // Nạp thêm thông tin tên và ảnh sản phẩm
            var productIds = topProducts.Select(p => p.ProductId).ToList();
            var productInfos = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Name, p.ImageUrl })
                .ToListAsync();

            foreach (var tp in topProducts)
            {
                var info = productInfos.FirstOrDefault(p => p.Id == tp.ProductId);
                if (info != null)
                {
                    tp.ProductName = info.Name;
                    tp.ImageUrl = info.ImageUrl;
                }
            }

            // === 5. 5 ĐƠN HÀNG MỚI NHẤT ===
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    CustomerName = o.Customer != null ? o.Customer.FullName : "Không xác định",
                    CustomerEmail = o.Customer != null ? o.Customer.Email : null,
                    TotalAmount = o.OrderDetails != null
                        ? o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice)
                        : 0,
                    Status = o.Status,
                    ItemCount = o.OrderDetails != null ? o.OrderDetails.Count() : 0
                })
                .ToListAsync();

            // === 6. ĐÓNG GÓI VIEWMODEL ===
            var model = new DashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                PendingOrders = pendingOrders,
                ShippingOrders = shippingOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                DailyRevenues = dailyRevenues,
                TopProducts = topProducts,
                RecentOrders = recentOrders,
                TotalCategories = totalCategories,
                RevenueFilter = revenueFilter,
                StartDate = startDate,
                EndDate = endDate,
                ChartTitle = chartTitle,
                ChartSubtitle = chartSubtitle
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
