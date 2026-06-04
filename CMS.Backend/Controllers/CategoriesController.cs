/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 04/06/2026
Mô tả: API Controller cung cấp danh sách chuyên mục bài viết cho Frontend ReactJS theo yêu cầu Buổi 7.
*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;

namespace CMS.Backend.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class CategoriesController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
      _context = context;
    }

    /// <summary>
    /// API lấy toàn bộ chuyên mục bài viết / tin tức.
    /// Đường dẫn: GET /api/Categories
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      try
      {
        var categories = await _context.Categories
          .OrderBy(c => c.Id)
          .Select(c => new
          {
            c.Id,
            c.Name,
            c.Description,
            PostCount = c.Posts.Count()
          })
          .ToListAsync();

        return Ok(categories);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new
        {
          message = "Lỗi hệ thống khi tải danh sách chuyên mục bài viết",
          detail = ex.Message
        });
      }
    }
  }
}
