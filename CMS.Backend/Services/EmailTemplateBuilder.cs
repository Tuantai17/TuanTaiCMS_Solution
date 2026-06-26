using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;
using CMS.Backend.Models;

namespace CMS.Backend.Services
{
    /// <summary>
    /// Sinh noi dung HTML cho cac loai email.
    /// Su dung inline CSS, font an toan, tien dinh dang vi-VN, escape du lieu nguoi dung.
    /// </summary>
    public static class EmailTemplateBuilder
    {
        private static readonly CultureInfo ViCulture = new("vi-VN");

        private static string Encode(string? value)
            => WebUtility.HtmlEncode(value ?? string.Empty);

        private static string FormatCurrency(decimal amount)
            => amount.ToString("N0", ViCulture) + " VNĐ";

        private static string WrapLayout(string title, string bodyContent)
        {
            return $@"
<!DOCTYPE html>
<html lang='vi'>
<head><meta charset='utf-8'/></head>
<body style='margin:0;padding:0;background-color:#f4f4f7;font-family:Arial,Helvetica,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f4f4f7;padding:30px 0;'>
    <tr><td align='center'>
      <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);'>
        
        <!-- Header -->
        <tr>
          <td style='background-color:#CF102D;padding:30px;text-align:center;'>
            <h1 style='color:#ffffff;margin:0;font-size:24px;letter-spacing:1px;'>{Encode(title)}</h1>
            <p style='color:#ffd6db;margin:10px 0 0 0;font-size:14px;'>Hệ thống MyKingdom</p>
          </td>
        </tr>

        <!-- Body -->
        <tr>
          <td style='padding:40px 30px;color:#333333;line-height:1.6;font-size:15px;'>
            {bodyContent}
          </td>
        </tr>

        <!-- Footer -->
        <tr>
          <td style='background-color:#f8f9fa;padding:25px;text-align:center;border-top:1px solid #eeeeee;'>
            <p style='margin:0;color:#6c757d;font-size:13px;line-height:1.5;'>
              Đây là email tự động từ hệ thống, vui lòng không trả lời qua email này.<br>
              © {DateTime.Now.Year} TuanTaiCMS. Tất cả quyền được bảo lưu.
            </p>
          </td>
        </tr>

      </table>
    </td></tr>
  </table>
</body>
</html>";
        }

        /// <summary>
        /// Template email xac nhan don hang.
        /// </summary>
        public static string BuildOrderConfirmationTemplate(OrderEmailModel model)
        {
            var itemsHtml = "";
            foreach (var item in model.Items)
            {
                itemsHtml += $@"
                <tr>
                  <td style='border:1px solid #eee;padding:10px;text-align:left;font-size:13px;color:#333;'>{Encode(item.ProductName)}</td>
                  <td style='border:1px solid #eee;padding:10px;text-align:center;font-size:13px;'>{item.Quantity}</td>
                  <td style='border:1px solid #eee;padding:10px;text-align:right;font-size:13px;'>{FormatCurrency(item.UnitPrice)}</td>
                  <td style='border:1px solid #eee;padding:10px;text-align:right;font-size:13px;font-weight:600;'>{FormatCurrency(item.LineTotal)}</td>
                </tr>";
            }

            var body = $@"
            <p>Xin chào <strong>{Encode(model.CustomerName)}</strong>,</p>
            <p>Cảm ơn bạn đã đặt hàng tại <strong>Hệ thống MyKingdom</strong>. Đơn hàng của bạn đã được tiếp nhận thành công.</p>

            <table width='100%' cellpadding='0' cellspacing='0' style='margin:20px 0;font-size:13px;'>
              <tr><td style='padding:6px 0;'><strong>Mã đơn hàng:</strong></td><td style='padding:6px 0;color:#CF102D;font-weight:700;'>{Encode(model.OrderCode)}</td></tr>
              <tr><td style='padding:6px 0;'><strong>Ngày đặt:</strong></td><td style='padding:6px 0;'>{model.OrderDate:dd/MM/yyyy HH:mm}</td></tr>
              <tr><td style='padding:6px 0;'><strong>Email:</strong></td><td style='padding:6px 0;'>{Encode(model.CustomerEmail)}</td></tr>
              <tr><td style='padding:6px 0;'><strong>Số điện thoại:</strong></td><td style='padding:6px 0;'>{Encode(model.Phone)}</td></tr>
              <tr><td style='padding:6px 0;'><strong>Địa chỉ:</strong></td><td style='padding:6px 0;'>{Encode(model.Address)}</td></tr>
              <tr><td style='padding:6px 0;'><strong>Thanh toán:</strong></td><td style='padding:6px 0;'>{Encode(model.PaymentMethod)}</td></tr>
              <tr><td style='padding:6px 0;'><strong>Trạng thái:</strong></td><td style='padding:6px 0;'>{Encode(model.OrderStatus)}</td></tr>
            </table>

            <h3 style='color:#002664;border-bottom:1px solid #eee;padding-bottom:6px;margin-top:25px;'>Chi tiết sản phẩm</h3>
            <table width='100%' cellpadding='0' cellspacing='0' style='border-collapse:collapse;margin-bottom:20px;'>
              <thead>
                <tr style='background-color:#f2f2f2;'>
                  <th style='border:1px solid #eee;padding:10px;text-align:left;font-size:13px;'>Tên sản phẩm</th>
                  <th style='border:1px solid #eee;padding:10px;text-align:center;font-size:13px;width:60px;'>SL</th>
                  <th style='border:1px solid #eee;padding:10px;text-align:right;font-size:13px;width:110px;'>Đơn giá</th>
                  <th style='border:1px solid #eee;padding:10px;text-align:right;font-size:13px;width:120px;'>Thành tiền</th>
                </tr>
              </thead>
              <tbody>{itemsHtml}</tbody>
              <tfoot>
                <tr>
                  <td colspan='3' style='border:1px solid #eee;padding:12px;text-align:right;font-weight:700;font-size:14px;'>Tổng tiền:</td>
                  <td style='border:1px solid #eee;padding:12px;text-align:right;font-weight:700;color:#CF102D;font-size:15px;'>{FormatCurrency(model.TotalAmount)}</td>
                </tr>
              </tfoot>
            </table>

            <p>Chúc bạn và gia đình có những trải nghiệm tuyệt vời!</p>";

            return WrapLayout("Xác Nhận Đơn Hàng", body);
        }

        /// <summary>
        /// Template email thanh toan thanh cong.
        /// </summary>
        public static string BuildPaymentSuccessTemplate(PaymentSuccessEmailModel model)
        {
            var body = $@"
            <p>Xin chào <strong>{Encode(model.CustomerName)}</strong>,</p>
            <p>Thanh toán cho đơn hàng <strong style='color:#CF102D;'>{Encode(model.OrderCode)}</strong> đã được xác nhận thành công!</p>

            <table width='100%' cellpadding='0' cellspacing='0' style='margin:20px 0;background:#f8fff8;border:1px solid #d4edda;border-radius:6px;padding:16px;font-size:13px;'>
              <tr><td style='padding:6px 12px;'><strong>Mã đơn hàng:</strong></td><td style='padding:6px 12px;'>{Encode(model.OrderCode)}</td></tr>
              <tr><td style='padding:6px 12px;'><strong>Mã giao dịch:</strong></td><td style='padding:6px 12px;'>{Encode(model.TransactionCode)}</td></tr>
              <tr><td style='padding:6px 12px;'><strong>Phương thức:</strong></td><td style='padding:6px 12px;'>{Encode(model.PaymentMethod)}</td></tr>
              <tr><td style='padding:6px 12px;'><strong>Thời gian:</strong></td><td style='padding:6px 12px;'>{model.PaymentDate:dd/MM/yyyy HH:mm}</td></tr>
              <tr><td style='padding:6px 12px;'><strong>Số tiền:</strong></td><td style='padding:6px 12px;font-weight:700;color:#28a745;font-size:16px;'>{FormatCurrency(model.TotalAmount)}</td></tr>
              <tr><td style='padding:6px 12px;'><strong>Trạng thái:</strong></td><td style='padding:6px 12px;color:#28a745;font-weight:600;'>Đã thanh toán</td></tr>
            </table>

            <p>Đơn hàng của bạn đang được xử lý. Chúng tôi sẽ thông báo khi đơn hàng được giao.</p>";

            return WrapLayout("Thanh Toán Thành Công", body);
        }

        /// <summary>
        /// Template email giao hang thanh cong.
        /// </summary>
        public static string BuildDeliverySuccessTemplate(DeliverySuccessEmailModel model)
        {
            var body = $@"
            <p>Xin chào <strong>{Encode(model.CustomerName)}</strong>,</p>
            <p>Đơn hàng <strong style='color:#CF102D;'>{Encode(model.OrderCode)}</strong> đã được giao thành công!</p>

            <table width='100%' cellpadding='0' cellspacing='0' style='margin:20px 0;background:#f8fff8;border:1px solid #d4edda;border-radius:6px;padding:16px;font-size:13px;'>
              <tr><td style='padding:6px 12px;'><strong>Mã đơn hàng:</strong></td><td style='padding:6px 12px;'>{Encode(model.OrderCode)}</td></tr>
              <tr><td style='padding:6px 12px;'><strong>Ngày giao:</strong></td><td style='padding:6px 12px;'>{model.DeliveredDate:dd/MM/yyyy HH:mm}</td></tr>
              <tr><td style='padding:6px 12px;'><strong>Địa chỉ:</strong></td><td style='padding:6px 12px;'>{Encode(model.Address)}</td></tr>
              <tr><td style='padding:6px 12px;'><strong>Tổng tiền:</strong></td><td style='padding:6px 12px;font-weight:700;color:#CF102D;font-size:15px;'>{FormatCurrency(model.TotalAmount)}</td></tr>
            </table>

            <p>Vui lòng kiểm tra sản phẩm ngay khi nhận hàng. Nếu có bất kỳ vấn đề nào, hãy liên hệ với chúng tôi trong vòng 24 giờ.</p>
            <p>Cảm ơn bạn đã tin tưởng và lựa chọn Hệ thống MyKingdom!</p>";

            return WrapLayout("Giao Hàng Thành Công", body);
        }

        /// <summary>
        /// Template email quen mat khau (OTP).
        /// </summary>
        public static string BuildForgotPasswordTemplate(ForgotPasswordEmailModel model)
        {
            var body = $@"
            <p>Xin chào <strong>{Encode(model.CustomerName)}</strong>,</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn tại Hệ thống MyKingdom.</p>
            <p>Mã xác minh (OTP) của bạn là:</p>

            <div style='text-align:center;margin:25px 0;'>
              <span style='display:inline-block;background:#f5f5f5;color:#CF102D;padding:14px 32px;border-radius:6px;font-weight:700;font-size:24px;letter-spacing:4px;border:1px dashed #CF102D;'>
                {Encode(model.OtpCode)}
              </span>
            </div>

            <p style='font-size:13px;'>Mã xác minh này dùng để nhập trên website hoặc ứng dụng, giúp bạn thiết lập lại mật khẩu mới.</p>

            <div style='background:#fff3cd;border:1px solid #ffc107;border-radius:6px;padding:12px 16px;margin:20px 0;font-size:13px;'>
              <strong>Lưu ý:</strong> Mã OTP này sẽ hết hạn vào lúc <strong>{model.ExpiredAt:dd/MM/yyyy HH:mm}</strong>.
              Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này, mật khẩu của bạn vẫn an toàn.
            </div>";

            return WrapLayout("Mã OTP Đặt Lại Mật Khẩu", body);
        }
    }
}
