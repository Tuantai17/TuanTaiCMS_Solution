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
        public async Task<IActionResult> Index()
        {
            // === 1. THỐNG KÊ TỔNG QUAN ===
            var totalOrders = await _context.Orders.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var totalPosts = await _context.Posts.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();

            // Tổng doanh thu tính từ đơn hàng đã hoàn thành (Status = 2)
            var totalRevenue = await _context.OrderDetails
                .Where(od => od.Order != null && od.Order.Status == 2)
                .SumAsync(od => (decimal?)(od.Quantity * od.UnitPrice)) ?? 0;

            // === 2. THỐNG KÊ ĐƠN HÀNG THEO TRẠNG THÁI ===
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == 0);
            var shippingOrders = await _context.Orders.CountAsync(o => o.Status == 1);
            var completedOrders = await _context.Orders.CountAsync(o => o.Status == 2);
            var cancelledOrders = await _context.Orders.CountAsync(o => o.Status == 3);

            // === 3. BIỂU ĐỒ DOANH THU 7 NGÀY GẦN NHẤT ===
            var today = DateTime.Today;
            var sevenDaysAgo = today.AddDays(-6); // 7 ngày: hôm nay + 6 ngày trước

            var revenueRaw = await _context.Orders
                .Where(o => o.Status == 2 && o.OrderDate.Date >= sevenDaysAgo && o.OrderDate.Date <= today)
                .Join(_context.OrderDetails,
                    o => o.Id,
                    od => od.OrderId,
                    (o, od) => new { o.OrderDate, od.Quantity, od.UnitPrice })
                .GroupBy(x => x.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice),
                    OrderCount = g.Select(x => x.OrderDate).Distinct().Count()
                })
                .ToListAsync();

            // Tạo danh sách đầy đủ 7 ngày (kể cả ngày không có doanh thu)
            var dailyRevenues = new List<DailyRevenueDto>();
            for (int i = 0; i < 7; i++)
            {
                var date = sevenDaysAgo.AddDays(i);
                var match = revenueRaw.FirstOrDefault(r => r.Date == date);
                dailyRevenues.Add(new DailyRevenueDto
                {
                    Date = date.ToString("dd/MM"),
                    Revenue = match?.Revenue ?? 0,
                    OrderCount = match?.OrderCount ?? 0
                });
            }

            // === 4. TOP 5 SẢN PHẨM BÁN CHẠY (tính trên tất cả đơn hàng, trừ đơn hủy) ===
            var topProducts = await _context.OrderDetails
                .Where(od => od.Order != null && od.Order.Status != 3)
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
                TotalPosts = totalPosts,
                TotalCategories = totalCategories
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
