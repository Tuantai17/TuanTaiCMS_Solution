using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Helpers;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailHelper _emailHelper;

        // Lưu trữ mã khôi phục mật khẩu tạm thời: Email -> (Mã code, Hết hạn)
        private static readonly Dictionary<string, (string Code, DateTime Expiry)> ResetCodes = new();

        public AuthController(ApplicationDbContext context, EmailHelper emailHelper)
        {
            _context = context;
            _emailHelper = emailHelper;
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
                    Password = PasswordHelper.HashPassword(input.Password), // Mã hóa mật khẩu bằng BCrypt
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
                // Bước 1: Tìm khách hàng theo Email
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == input.Email);

                // Bước 2: Xác thực mật khẩu bằng BCrypt
                if (customer == null || !PasswordHelper.VerifyPassword(input.Password, customer.Password))
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

        /// <summary>
        /// API Bước 1: Gửi mã OTP 6 số xác minh qua email
        /// POST: api/Auth/SendResetCode
        /// </summary>
        [HttpPost("SendResetCode")]
        public async Task<IActionResult> SendResetCode([FromBody] SendResetCodeInputDTO input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Email))
            {
                return BadRequest(new { message = "Email không được để trống" });
            }

            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == input.Email);
                if (customer == null)
                {
                    return BadRequest(new { message = "Email này không tồn tại trong hệ thống!" });
                }

                // Sinh mã OTP 6 chữ số ngẫu nhiên
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();

                // Lưu vào Dictionary tạm thời (hết hạn sau 5 phút)
                ResetCodes[input.Email.Trim().ToLower()] = (otpCode, DateTime.Now.AddMinutes(5));

                // Soạn thảo Email HTML
                var htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; padding: 20px; border-radius: 10px;'>
                        <div style='text-align: center; border-bottom: 2px solid #CF102D; padding-bottom: 10px; margin-bottom: 20px;'>
                            <h2 style='color: #CF102D; margin: 0;'>MyKingdom - Mã Xác Minh Khôi Phục Mật Khẩu</h2>
                        </div>
                        <p>Xin chào <strong>{customer.FullName}</strong>,</p>
                        <p>Chúng tôi nhận được yêu cầu khôi phục mật khẩu cho tài khoản của bạn tại <strong>Vương Quốc Đồ Chơi MyKingdom</strong>.</p>
                        <p>Mã xác minh (OTP) của bạn là:</p>
                        <div style='background-color: #f9f9f9; border-left: 4px solid #CF102D; padding: 15px; text-align: center; margin: 20px 0;'>
                            <span style='color: #CF102D; font-size: 2em; font-weight: bold; letter-spacing: 5px;'>{otpCode}</span>
                        </div>
                        <p style='color: #666;'>Mã xác minh này có hiệu lực trong vòng <strong>5 phút</strong>. Vui lòng không chia sẻ mã này cho bất kỳ ai khác.</p>
                        <p style='font-size: 0.9em; color: #666; text-align: center; border-top: 1px solid #eee; padding-top: 15px; margin-top: 25px;'>
                            Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.<br/>
                            Trân trọng cảm ơn!
                        </p>
                    </div>
                ";

                // Gửi email xác minh trực tiếp và đợi hoàn thành
                bool emailSent = true;
                string emailErrorMessage = null;
                try
                {
                    await _emailHelper.SendEmailAsync(customer.Email.Trim(), "[MyKingdom] Mã xác minh khôi phục mật khẩu", htmlBody);
                }
                catch (Exception emailEx)
                {
                    emailSent = false;
                    emailErrorMessage = emailEx.Message;
                    Console.WriteLine($">>> Lỗi gửi email OTP: {emailEx.Message}");
                }

                if (!emailSent)
                {
                    // Trả về thành công kèm OTP để dễ test/phục hồi mật khẩu nếu cấu hình SMTP lỗi
                    return Ok(new { 
                        message = $"Không thể gửi email OTP (Lỗi cấu hình SMTP hoặc kết nối mạng). Dưới đây là mã xác minh của bạn để tiếp tục kiểm thử: {otpCode}",
                        otpForTesting = otpCode,
                        isTestMode = true,
                        errorDetail = emailErrorMessage
                    });
                }

                // Trả về thành công kèm OTP để dễ test nếu không có email/mạng
                return Ok(new { 
                    message = "Mã xác minh đã được gửi về Gmail của bạn!",
                    otpForTesting = otpCode // Hỗ trợ test nhanh trực tiếp trên Frontend nếu chưa nhận được email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý gửi mã OTP", detail = ex.Message });
            }
        }

        /// <summary>
        /// API Bước 2: Kiểm tra xác thực mã OTP
        /// POST: api/Auth/VerifyResetCode
        /// </summary>
        [HttpPost("VerifyResetCode")]
        public IActionResult VerifyResetCode([FromBody] VerifyResetCodeInputDTO input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Code))
            {
                return BadRequest(new { message = "Email và Mã xác minh không được để trống" });
            }

            var emailKey = input.Email.Trim().ToLower();
            if (!ResetCodes.ContainsKey(emailKey))
            {
                return BadRequest(new { message = "Mã xác minh không tồn tại hoặc đã hết hạn. Vui lòng bấm gửi lại mã." });
            }

            var (storedCode, expiry) = ResetCodes[emailKey];

            if (DateTime.Now > expiry)
            {
                ResetCodes.Remove(emailKey);
                return BadRequest(new { message = "Mã xác minh đã hết hạn (5 phút). Vui lòng yêu cầu mã mới." });
            }

            if (storedCode != input.Code.Trim())
            {
                return BadRequest(new { message = "Mã xác minh không chính xác. Vui lòng kiểm tra lại." });
            }

            return Ok(new { message = "Xác thực mã OTP thành công! Vui lòng đặt mật khẩu mới." });
        }

        /// <summary>
        /// API Bước 3: Đặt mật khẩu mới
        /// POST: api/Auth/ResetPassword
        /// </summary>
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordInputDTO input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Code) || string.IsNullOrWhiteSpace(input.NewPassword))
            {
                return BadRequest(new { message = "Thông tin yêu cầu không được để trống" });
            }

            try
            {
                var emailKey = input.Email.Trim().ToLower();
                if (!ResetCodes.ContainsKey(emailKey))
                {
                    return BadRequest(new { message = "Yêu cầu khôi phục không hợp lệ hoặc đã hết hạn." });
                }

                var (storedCode, expiry) = ResetCodes[emailKey];

                if (DateTime.Now > expiry)
                {
                    ResetCodes.Remove(emailKey);
                    return BadRequest(new { message = "Yêu cầu khôi phục đã hết hạn. Vui lòng thực hiện lại từ đầu." });
                }

                if (storedCode != input.Code.Trim())
                {
                    return BadRequest(new { message = "Mã xác minh không chính xác." });
                }

                // Tìm khách hàng trong cơ sở dữ liệu
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == input.Email);
                if (customer == null)
                {
                    return BadRequest(new { message = "Không tìm thấy thông tin tài khoản." });
                }

                // Hash mật khẩu mới bằng BCrypt và lưu lại DB
                customer.Password = PasswordHelper.HashPassword(input.NewPassword);
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                // Xóa mã OTP khỏi Dictionary sau khi đổi thành công
                ResetCodes.Remove(emailKey);

                return Ok(new { message = "Đổi mật khẩu mới thành công! Bạn có thể dùng mật khẩu này để đăng nhập." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý đặt mật khẩu mới", detail = ex.Message });
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

    public class SendResetCodeInputDTO
    {
        public string Email { get; set; }
    }

    public class VerifyResetCodeInputDTO
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class ResetPasswordInputDTO
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}
