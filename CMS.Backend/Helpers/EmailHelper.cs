using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CMS.Backend.Helpers
{
    public class EmailHelper
    {
        private readonly IConfiguration _configuration;

        public EmailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gửi email HTML bất đồng bộ sử dụng thư viện MailKit (thay thế System.Net.Mail đã lỗi thời).
        /// MailKit hỗ trợ STARTTLS/OAuth2 tốt hơn và được Microsoft khuyến nghị chính thức.
        /// </summary>
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
            var port = int.Parse(emailSettings["Port"] ?? "587");
            var senderName = emailSettings["SenderName"] ?? "MyKingdom";
            var senderEmail = emailSettings["SenderEmail"] ?? "";
            var senderPassword = emailSettings["SenderPassword"] ?? "";

            // Tạo đối tượng email MimeMessage
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail.Trim()));
            message.Subject = subject;

            // Thiết lập nội dung HTML
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            message.Body = bodyBuilder.ToMessageBody();

            // Kết nối và gửi email qua MailKit SmtpClient
            using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
            {
                // Kết nối tới SMTP Server sử dụng STARTTLS (port 587)
                await smtpClient.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);

                // Xác thực bằng email + App Password
                await smtpClient.AuthenticateAsync(senderEmail, senderPassword);

                // Gửi email
                await smtpClient.SendAsync(message);

                // Ngắt kết nối
                await smtpClient.DisconnectAsync(true);
            }
        }
    }
}
