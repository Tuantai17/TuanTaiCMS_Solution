/*
Sinh Viên: Nguyễn Tuấn Tài
Mã Sinh Viên: 2123110166
Lớp: CCQ2311E
Ngày Tạo: 5/6/2026
Mô tả: Class tiện ích mã hóa và xác thực mật khẩu bằng thuật toán BCrypt.
        Tập trung logic hash tại một nơi, tránh lặp code trong các Controller.
*/

namespace CMS.Backend.Helpers
{
    /// <summary>
    /// Cung cấp các phương thức tĩnh để mã hóa (hash) và xác thực (verify) mật khẩu.
    /// Sử dụng thuật toán BCrypt — tự động sinh salt ngẫu nhiên và chống brute-force.
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Mã hóa mật khẩu plain text thành chuỗi hash BCrypt.
        /// Kết quả trả về dạng: $2a$11$xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        /// </summary>
        /// <param name="password">Mật khẩu dạng chữ thô từ người dùng nhập vào.</param>
        /// <returns>Chuỗi hash BCrypt đã mã hóa, sẵn sàng lưu vào database.</returns>
        public static string HashPassword(string password)
        {
            // WorkFactor mặc định = 11 (cân bằng giữa bảo mật và tốc độ)
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// So sánh mật khẩu plain text với chuỗi hash đã lưu trong database.
        /// BCrypt tự trích xuất salt từ hash để so sánh chính xác.
        /// </summary>
        /// <param name="password">Mật khẩu plain text mà người dùng nhập khi đăng nhập.</param>
        /// <param name="hashedPassword">Chuỗi hash BCrypt đã lưu trong database.</param>
        /// <returns>true nếu mật khẩu khớp, false nếu sai.</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                // Trường hợp hash bị lỗi định dạng (ví dụ: dữ liệu cũ chưa được migrate)
                // Trả về false để tránh crash hệ thống
                return false;
            }
        }
    }
}
