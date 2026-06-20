using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
                // Tránh kiểm tra danh sách thu hồi chứng chỉ (CRL) vốn rất chậm hoặc bị chặn ở một số mạng
                smtpClient.CheckCertificateRevocation = false;

                // Thiết lập thời gian chờ tối đa 5 giây cho các thao tác mạng (mặc định là 100 giây)
                smtpClient.Timeout = 5000;

                try
                {
                    // Giải quyết địa chỉ IP và lọc lấy IPv4 để tránh lỗi treo do IPv6 cục bộ bị chặn/lỗi định tuyến
                    var ipAddresses = await Dns.GetHostAddressesAsync(smtpServer);
                    var ipv4Address = ipAddresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                    if (ipv4Address != null)
                    {
                        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        {
                            SendTimeout = 5000,
                            ReceiveTimeout = 5000
                        };

                        // Kết nối socket bất đồng bộ
                        await socket.ConnectAsync(new IPEndPoint(ipv4Address, port));

                        // Truyền socket đã kết nối cho MailKit và kích hoạt STARTTLS (kiểm tra SSL trùng khớp với hostname)
                        await smtpClient.ConnectAsync(socket, smtpServer, port, SecureSocketOptions.StartTls);
                    }
                    else
                    {
                        // Phương án dự phòng mặc định nếu không phân giải được IPv4
                        await smtpClient.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi và thử kết nối trực tiếp bằng cơ chế mặc định của MailKit làm phương án dự phòng
                    Console.WriteLine($">>> Lỗi kết nối IPv4 tối ưu: {ex.Message}. Đang thử kết nối dự phòng mặc định...");
                    
                    // Tạo mới thực thể SmtpClient để tránh trạng thái không nhất quán của Socket cũ
                    using (var backupClient = new MailKit.Net.Smtp.SmtpClient())
                    {
                        backupClient.CheckCertificateRevocation = false;
                        backupClient.Timeout = 5000;

                        await backupClient.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
                        await backupClient.AuthenticateAsync(senderEmail, senderPassword);
                        await backupClient.SendAsync(message);
                        await backupClient.DisconnectAsync(true);
                        return;
                    }
                }

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
