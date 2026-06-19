/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 5/6/2026
Mô tả: Controller tạm thời để migration mật khẩu plain text sang BCrypt hash.
        Chạy MỘT LẦN DUY NHẤT sau khi deploy code mới, sau đó XÓA file này.
*/

using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using CMS.Backend.Helpers;

namespace CMS.Backend.Controllers
{
    /// <summary>
    /// API tạm thời để hash toàn bộ mật khẩu plain text trong database.
    /// GỌI MỘT LẦN DUY NHẤT qua Swagger, sau đó XÓA file này khỏi dự án.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MigratePasswordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MigratePasswordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hash toàn bộ mật khẩu plain text của Users và Customers.
        /// Chỉ hash các mật khẩu chưa có prefix "$2" (chưa phải BCrypt hash).
        /// POST: api/MigratePasswords/Run
        /// </summary>
        [HttpPost("Run")]
        public IActionResult Run()
        {
            int usersUpdated = 0;
            int customersUpdated = 0;

            // --- Hash mật khẩu bảng Users ---
            var users = _context.Users.ToList();
            foreach (var user in users)
            {
                // Kiểm tra: nếu chưa phải BCrypt hash (không bắt đầu bằng "$2")
                if (!string.IsNullOrEmpty(user.PasswordHash) && !user.PasswordHash.StartsWith("$2"))
                {
                    user.PasswordHash = PasswordHelper.HashPassword(user.PasswordHash);
                    usersUpdated++;
                }
            }

            // --- Hash mật khẩu bảng Customers ---
            var customers = _context.Customers.ToList();
            foreach (var customer in customers)
            {
                // Kiểm tra: nếu chưa phải BCrypt hash (không bắt đầu bằng "$2")
                if (!string.IsNullOrEmpty(customer.Password) && !customer.Password.StartsWith("$2"))
                {
                    customer.Password = PasswordHelper.HashPassword(customer.Password);
                    customersUpdated++;
                }
            }

            _context.SaveChanges();

            return Ok(new
            {
                message = "Migration mật khẩu hoàn tất!",
                usersUpdated,
                customersUpdated,
                warning = "Hãy XÓA file MigratePasswordsController.cs khỏi dự án sau khi chạy xong."
            });
        }
    }
}
