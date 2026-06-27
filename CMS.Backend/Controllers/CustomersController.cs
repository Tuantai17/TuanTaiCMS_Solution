using CMS.Backend.Helpers;
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CustomersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CustomerRegisterDto model)
        {
            if (model == null ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.FullName))
            {
                return BadRequest(new { message = "Vui lòng điền đầy đủ các thông tin bắt buộc!" });
            }

            try
            {
                var normalizedEmail = model.Email.Trim().ToLower();
                var isEmailExist = await _context.Customers
                    .AnyAsync(c => c.Email != null && c.Email.Trim().ToLower() == normalizedEmail);

                if (isEmailExist)
                {
                    return BadRequest(new { message = "Email này đã được đăng ký trên hệ thống MyKingdom!" });
                }

                var newCustomer = new Customer
                {
                    FullName = model.FullName.Trim(),
                    Email = model.Email.Trim(),
                    Phone = model.Phone,
                    Address = model.Address,
                    Password = PasswordHelper.HashPassword(model.Password),
                };

                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();

                return StatusCode(201, new
                {
                    message = "Tạo tài khoản khách hàng thành công!",
                    customerId = newCustomer.Id,
                    fullName = newCustomer.FullName,
                    email = newCustomer.Email,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống khi đăng ký: {ex.Message}" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] CustomerLoginDto model)
        {
            if (model == null ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest(new { message = "Tài khoản và mật khẩu không được để trống!" });
            }

            try
            {
                var normalizedEmail = model.Email.Trim().ToLower();
                var customer = await _context.Customers.FirstOrDefaultAsync(
                    c => c.Email.Trim().ToLower() == normalizedEmail);

                if (customer == null || !PasswordHelper.VerifyPassword(model.Password, customer.Password))
                {
                    return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
                }

                var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
                var accessToken = CustomerSessionTokenHelper.GenerateToken(
                    customer.Id,
                    customer.Email,
                    GetCustomerSessionSecret(),
                    expiresAt);

                return Ok(new
                {
                    id = customer.Id,
                    customerId = customer.Id,
                    fullName = customer.FullName,
                    email = customer.Email,
                    phone = customer.Phone,
                    address = customer.Address,
                    avatarUrl = customer.AvatarUrl,
                    accessToken,
                    expiresAt = expiresAt.UtcDateTime,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống khi xác thực: {ex.Message}" });
            }
        }

        private string GetCustomerSessionSecret()
        {
            return _configuration["CustomerSession:Secret"]
                ?? "TuanTaiCMS.CustomerSession.Secret.2026";
        }
    }

    public class CustomerRegisterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public class CustomerLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
