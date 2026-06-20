using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Helpers;
using System.Text.RegularExpressions;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailHelper _emailHelper;
        private readonly IConfiguration _configuration;

        // Lưu trữ mã khôi phục mật khẩu tạm thời: Email -> (Mã code, Hết hạn)
        private static readonly Dictionary<string, (string Code, DateTime Expiry)> ResetCodes = new();

        public AuthController(ApplicationDbContext context, EmailHelper emailHelper, IConfiguration configuration)
        {
            _context = context;
            _emailHelper = emailHelper;
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

                // Lưu vào Dictionary tạm thời (hết hạn sau 5 phút)
                ResetCodes[input.Email.Trim().ToLower()] = (otpCode, DateTime.Now.AddMinutes(5));

                // Soạn thảo Email HTML đơn giản hơn để tránh bị bộ lọc Spam của Gmail nhận diện nhầm là giả mạo thương hiệu (phishing)
                var htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 15px; color: #333;'>
                        <p>Xin chào <strong>{customer.FullName}</strong>,</p>
                        <p>Chúng tôi nhận được yêu cầu lấy lại mật khẩu cho tài khoản của bạn tại ứng dụng thử nghiệm MyKingdom.</p>
                        <p>Mã xác minh (OTP) của bạn là:</p>
                        <p style='font-size: 24px; font-weight: bold; color: #CF102D; letter-spacing: 3px; padding: 10px 20px; background: #f5f5f5; display: inline-block; border-radius: 5px; margin: 10px 0;'>{otpCode}</p>
                        <p style='font-size: 0.9em; color: #666;'>Mã xác minh này có hiệu lực trong vòng <strong>5 phút</strong>. Vui lòng không chia sẻ mã này cho bất kỳ ai khác.</p>
                        <p style='font-size: 0.9em; color: #666;'>Nếu bạn không thực hiện yêu cầu này, bạn có thể an tâm bỏ qua email này.</p>
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                        <p style='font-size: 0.85em; color: #999;'>Đây là email tự động từ hệ thống thử nghiệm MyKingdom. Vui lòng không phản hồi email này.</p>
                    </div>
                ";

                // Gửi email xác minh trực tiếp và đợi hoàn thành
                bool emailSent = true;
                string emailErrorMessage = null;
                try
                {
                    await _emailHelper.SendEmailAsync(customer.Email.Trim(), "[MyKingdom] Ma OTP khoi phuc mat khau", htmlBody);
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
