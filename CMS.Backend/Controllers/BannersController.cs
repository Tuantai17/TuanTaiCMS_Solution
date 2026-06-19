/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 18/6/2026
Mô tả: API Controller cung cấp dữ liệu danh sách Banner đang được phép hiển thị cho trang chủ React.
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BannersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách banner đang hiển thị (IsVisible = true)
        // Đường dẫn truy cập: GET https://localhost:xxxx/api/banners
        [HttpGet]
        public async Task<IActionResult> GetActiveBanners()
        {
            try
            {
                var banners = await _context.Banners
                    .Where(b => b.IsVisible)
                    .OrderBy(b => b.DisplayOrder)
                    .ThenByDescending(b => b.CreatedDate)
                    .Select(b => new
                    {
                        b.Id,
                        b.Title,
                        b.ImageUrl,
                        b.DisplayOrder,
                        b.TargetUrl
                    })
                    .ToListAsync();

                return Ok(banners);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý hệ thống khi lấy danh sách banner", detail = ex.Message });
            }
        }
    }
}
