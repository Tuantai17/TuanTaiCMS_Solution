using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// API Đăng ký tài khoản khách hàng mới
        /// POST: api/Auth/CustomerRegister
        /// </summary>
        [HttpPost("CustomerRegister")]
        public async Task<IActionResult> Register([FromBody] RegisterInputDTO input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dữ liệu đăng ký không hợp lệ" });
            }

            if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Password) || string.IsNullOrWhiteSpace(input.FullName))
            {
                return BadRequest(new { message = "Họ tên, Email và Mật khẩu không được để trống" });
            }

            try
            {
                // Kiểm tra Email trùng lặp
                var emailExist = await _context.Customers.AnyAsync(c => c.Email == input.Email);
                if (emailExist)
                {
                    return BadRequest(new { message = "Email này đã được đăng ký trong hệ thống!" });
                }

                // Khởi tạo đối tượng Customer mới
                var customer = new Customer
                {
                    FullName = input.FullName,
                    Email = input.Email,
                    Password = input.Password, // Lưu thô tối giản theo yêu cầu chốt điểm
                    Phone = input.Phone,
                    Address = input.Address
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Đăng ký tài khoản thành công!",
                    customerId = customer.Id,
                    fullName = customer.FullName,
                    email = customer.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý đăng ký tài khoản khách hàng", detail = ex.Message });
            }
        }

        /// <summary>
        /// API Đăng nhập khách hàng
        /// POST: api/Auth/CustomerLogin
        /// </summary>
        [HttpPost("CustomerLogin")]
        public async Task<IActionResult> Login([FromBody] LoginInputDTO input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dữ liệu đăng nhập không hợp lệ" });
            }

            if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Password))
            {
                return BadRequest(new { message = "Email và Mật khẩu không được để trống" });
            }

            try
            {
                // Xác thực tài khoản khách hàng từ Database
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == input.Email && c.Password == input.Password);

                if (customer == null)
                {
                    return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác!" });
                }

                // Trả về thông tin khách hàng để lưu ở Client
                return Ok(new
                {
                    message = "Đăng nhập thành công!",
                    customerId = customer.Id,
                    fullName = customer.FullName,
                    email = customer.Email,
                    phone = customer.Phone,
                    address = customer.Address
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý đăng nhập hệ thống", detail = ex.Message });
            }
        }
    }

    public class RegisterInputDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class LoginInputDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
