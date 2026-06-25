using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Helpers;
using CMS.Backend.Services;
using System.Text.RegularExpressions;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
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
                var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
                var accessToken = CustomerSessionTokenHelper.GenerateToken(
                    customer.Id,
                    customer.Email,
                    GetCustomerSessionSecret(),
                    expiresAt);

                return Ok(new
                {
                    message = "Đăng nhập thành công!",
                    customerId = customer.Id,
                    fullName = customer.FullName,
                    email = customer.Email,
                    phone = customer.Phone,
                    address = customer.Address,
                    dateOfBirth = customer.DateOfBirth,
                    gender = customer.Gender,
                    avatarUrl = customer.AvatarUrl,
                    createdAt = customer.CreatedAt,
                    accessToken,
                    expiresAt = expiresAt.UtcDateTime
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

                // Luu vao database voi ma hoa SHA256
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(otpCode);
                    var hash = Convert.ToBase64String(sha256.ComputeHash(bytes));

                    // Vo hieu hoa cac token cu
                    var oldTokens = await _context.PasswordResetTokens
                        .Where(t => t.CustomerId == customer.Id && !t.IsUsed && t.ExpiredAt > DateTime.Now)
                        .ToListAsync();
                    foreach (var t in oldTokens) { t.IsUsed = true; }

                    var token = new PasswordResetToken
                    {
                        CustomerId = customer.Id,
                        TokenHash = hash,
                        CreatedAt = DateTime.Now,
                        ExpiredAt = DateTime.Now.AddMinutes(5),
                        IsUsed = false
                    };
                    _context.PasswordResetTokens.Add(token);
                    await _context.SaveChangesAsync();
                }

                // Gui email thong qua IEmailService
                var emailModel = new CMS.Backend.Models.ForgotPasswordEmailModel
                {
                    CustomerName = customer.FullName,
                    CustomerEmail = customer.Email,
                    OtpCode = otpCode,
                    ExpiredAt = DateTime.Now.AddMinutes(5)
                };

                bool emailSent = await _emailService.SendForgotPasswordAsync(emailModel);

                if (!emailSent)
                {
                    // Trả về thành công kèm OTP để dễ test/phục hồi mật khẩu nếu cấu hình SMTP lỗi
                    return Ok(new { 
                        message = $"Không thể gửi email OTP. Dưới đây là mã xác minh của bạn để tiếp tục kiểm thử: {otpCode}",
                        otpForTesting = otpCode,
                        isTestMode = true
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
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeInputDTO input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Code))
            {
                return BadRequest(new { message = "Email và Mã xác minh không được để trống" });
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == input.Email);
            if (customer == null) return BadRequest(new { message = "Email không hợp lệ." });

            string hash;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(input.Code.Trim());
                hash = Convert.ToBase64String(sha256.ComputeHash(bytes));
            }

            var token = await _context.PasswordResetTokens
                .Where(t => t.CustomerId == customer.Id && t.TokenHash == hash && !t.IsUsed)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (token == null)
            {
                return BadRequest(new { message = "Mã xác minh không tồn tại hoặc đã bị thay đổi." });
            }

            if (DateTime.Now > token.ExpiredAt)
            {
                return BadRequest(new { message = "Mã xác minh đã hết hạn (5 phút). Vui lòng yêu cầu mã mới." });
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
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == input.Email);
                if (customer == null) return BadRequest(new { message = "Email không hợp lệ." });

                string hash;
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(input.Code.Trim());
                    hash = Convert.ToBase64String(sha256.ComputeHash(bytes));
                }

                var token = await _context.PasswordResetTokens
                    .Where(t => t.CustomerId == customer.Id && t.TokenHash == hash && !t.IsUsed)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync();

                if (token == null)
                {
                    return BadRequest(new { message = "Yêu cầu khôi phục không hợp lệ." });
                }

                if (DateTime.Now > token.ExpiredAt)
                {
                    return BadRequest(new { message = "Yêu cầu khôi phục đã hết hạn. Vui lòng thực hiện lại từ đầu." });
                }

                // Cập nhật mật khẩu mới
                customer.Password = PasswordHelper.HashPassword(input.NewPassword);
                
                // Đánh dấu token đã sử dụng
                token.IsUsed = true;
                token.UsedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Đổi mật khẩu mới thành công! Bạn có thể dùng mật khẩu này để đăng nhập." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý đặt mật khẩu mới", detail = ex.Message });
            }
        }

        /// <summary>
        /// API Lấy thông tin cá nhân khách hàng
        /// GET: api/Auth/GetProfile/{id}
        /// </summary>
        [HttpGet("GetProfile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Orders)
                    .FirstOrDefaultAsync(c => c.Id == id);
                if (customer == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
                }

                return Ok(new
                {
                    customerId = customer.Id,
                    fullName = customer.FullName,
                    email = customer.Email,
                    phone = customer.Phone,
                    address = customer.Address,
                    dateOfBirth = customer.DateOfBirth,
                    gender = customer.Gender,
                    avatarUrl = customer.AvatarUrl,
                    createdAt = customer.CreatedAt,
                    totalOrders = customer.Orders != null ? customer.Orders.Count : 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý lấy thông tin khách hàng", detail = ex.Message });
            }
        }

        /// <summary>
        /// API Cập nhật thông tin cá nhân khách hàng (Hỗ trợ thay đổi cả mật khẩu nếu có)
        /// POST: api/Auth/UpdateProfile
        /// </summary>
        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileInputDTO input)
        {
            if (input == null || input.CustomerId <= 0)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            if (string.IsNullOrWhiteSpace(input.FullName) || string.IsNullOrWhiteSpace(input.Email))
            {
                return BadRequest(new { message = "Họ tên và Email không được để trống" });
            }

            try
            {
                var customer = await _context.Customers.FindAsync(input.CustomerId);
                if (customer == null)
                {
                    return NotFound(new { message = "Không tìm thấy tài khoản khách hàng" });
                }

                // Kiểm tra Email trùng với người khác
                var emailExist = await _context.Customers.AnyAsync(c => c.Email == input.Email && c.Id != input.CustomerId);
                if (emailExist)
                {
                    return BadRequest(new { message = "Email này đã được sử dụng bởi tài khoản khác!" });
                }

                customer.FullName = input.FullName;
                customer.Email = input.Email;
                customer.Phone = input.Phone;
                customer.Address = input.Address;
                customer.DateOfBirth = input.DateOfBirth;
                customer.Gender = input.Gender;

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật thông tin tài khoản thành công!",
                    customerId = customer.Id,
                    fullName = customer.FullName,
                    email = customer.Email,
                    phone = customer.Phone,
                    address = customer.Address,
                    dateOfBirth = customer.DateOfBirth,
                    gender = customer.Gender,
                    avatarUrl = customer.AvatarUrl,
                    createdAt = customer.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý cập nhật thông tin tài khoản", detail = ex.Message });
            }
        }

        /// <summary>
        /// API Đổi mật khẩu tài khoản khách hàng
        /// POST: api/Auth/ChangePassword
        /// </summary>
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordInputDTO input)
        {
            if (input == null || input.CustomerId <= 0)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            if (string.IsNullOrWhiteSpace(input.OldPassword) || string.IsNullOrWhiteSpace(input.NewPassword))
            {
                return BadRequest(new { message = "Vui lòng điền đầy đủ mật khẩu hiện tại và mật khẩu mới" });
            }

            if (input.NewPassword.Length < 8)
            {
                return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 8 ký tự" });
            }

            if (!IsStrongPassword(input.NewPassword))
            {
                return BadRequest(new { message = "Mật khẩu mới phải gồm chữ hoa, chữ thường, số và ký tự đặc biệt" });
            }

            try
            {
                var customer = await _context.Customers.FindAsync(input.CustomerId);
                if (customer == null)
                {
                    return NotFound(new { message = "Không tìm thấy tài khoản khách hàng" });
                }

                if (!PasswordHelper.VerifyPassword(input.OldPassword, customer.Password))
                {
                    return BadRequest(new { message = "Mật khẩu hiện tại không chính xác!" });
                }

                if (input.OldPassword == input.NewPassword)
                {
                    return BadRequest(new { message = "Mật khẩu mới không được trùng với mật khẩu hiện tại" });
                }

                customer.Password = PasswordHelper.HashPassword(input.NewPassword);
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý đổi mật khẩu", detail = ex.Message });
            }
        }

        /// <summary>
        /// API Upload ảnh đại diện khách hàng
        /// POST: api/Auth/UploadAvatar
        /// </summary>
        [HttpPost("UploadAvatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarInputDTO input)
        {
            if (input == null || input.CustomerId <= 0 || input.Avatar == null || input.Avatar.Length == 0)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ hoặc chưa chọn ảnh" });
            }

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/jpg" };
            if (!allowedTypes.Contains(input.Avatar.ContentType.ToLower()))
            {
                return BadRequest(new { message = "Chỉ chấp nhận file ảnh JPG, PNG hoặc WEBP" });
            }

            if (input.Avatar.Length > 2 * 1024 * 1024)
            {
                return BadRequest(new { message = "Dung lượng ảnh tối đa 2MB" });
            }

            try
            {
                var customer = await _context.Customers.FindAsync(input.CustomerId);
                if (customer == null)
                {
                    return NotFound(new { message = "Không tìm thấy tài khoản khách hàng" });
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileExt = Path.GetExtension(input.Avatar.FileName);
                var fileName = $"avatar_{input.CustomerId}_{DateTime.Now:yyyyMMddHHmmss}{fileExt}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await input.Avatar.CopyToAsync(stream);
                }

                customer.AvatarUrl = $"/uploads/avatars/{fileName}";
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật ảnh đại diện thành công!",
                    avatarUrl = customer.AvatarUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý upload ảnh đại diện", detail = ex.Message });
            }
        }

        private static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            const string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }

        private string GetCustomerSessionSecret()
        {
            return _configuration["CustomerSession:Secret"]
                ?? "TuanTaiCMS.CustomerSession.Secret.2026";
        }
    }

    public class UploadAvatarInputDTO
    {
        public int CustomerId { get; set; }
        public IFormFile Avatar { get; set; }
    }

    public class UpdateProfileInputDTO
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }

    public class ChangePasswordInputDTO
    {
        public int CustomerId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
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
