using System.Net;
using System.Net.Sockets;
using CMS.Backend.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CMS.Backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                _logger.LogWarning("SendEmailAsync: Dia chi email nguoi nhan rong.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_settings.SenderEmail) || string.IsNullOrWhiteSpace(_settings.Password))
            {
                _logger.LogWarning("SendEmailAsync: Chua cau hinh SenderEmail hoac Password trong EmailSettings.");
                return false;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(recipientEmail.Trim()));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                using var smtpClient = new SmtpClient();
                smtpClient.CheckCertificateRevocation = false;
                smtpClient.Timeout = 10000;

                var sslOption = _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

                try
                {
                    // Thu ket noi qua IPv4 truoc de tranh loi IPv6 bi chan
                    var ipAddresses = await Dns.GetHostAddressesAsync(_settings.SmtpServer, cancellationToken);
                    var ipv4 = ipAddresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                    if (ipv4 != null)
                    {
                        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        {
                            SendTimeout = 10000,
                            ReceiveTimeout = 10000
                        };
                        await socket.ConnectAsync(new IPEndPoint(ipv4, _settings.Port), cancellationToken);
                        await smtpClient.ConnectAsync(socket, _settings.SmtpServer, _settings.Port, sslOption, cancellationToken);
                    }
                    else
                    {
                        await smtpClient.ConnectAsync(_settings.SmtpServer, _settings.Port, sslOption, cancellationToken);
                    }
                }
                catch (Exception connEx)
                {
                    _logger.LogWarning("IPv4 connect failed: {Msg}. Trying default...", connEx.Message);
                    // Tao SmtpClient moi de tranh trang thai khong nhat quan
                    using var backupClient = new SmtpClient();
                    backupClient.CheckCertificateRevocation = false;
                    backupClient.Timeout = 10000;

                    await backupClient.ConnectAsync(_settings.SmtpServer, _settings.Port, sslOption, cancellationToken);
                    var username = !string.IsNullOrWhiteSpace(_settings.Username) ? _settings.Username : _settings.SenderEmail;
                    await backupClient.AuthenticateAsync(username, _settings.Password, cancellationToken);
                    await backupClient.SendAsync(message, cancellationToken);
                    await backupClient.DisconnectAsync(true, cancellationToken);

                    _logger.LogInformation("Email sent successfully (backup) to {Email}", recipientEmail);
                    return true;
                }

                var authUsername = !string.IsNullOrWhiteSpace(_settings.Username) ? _settings.Username : _settings.SenderEmail;
                await smtpClient.AuthenticateAsync(authUsername, _settings.Password, cancellationToken);
                await smtpClient.SendAsync(message, cancellationToken);
                await smtpClient.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email sent successfully to {Email}", recipientEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loi gui email toi {Email}: {Message}", recipientEmail, ex.Message);
                System.IO.File.AppendAllText("e:\\ASP.NET\\TuanTaiCMS_Solution\\email_error.txt", $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n\n");
                return false;
            }
        }

        public async Task<bool> SendOrderConfirmationAsync(OrderEmailModel model, CancellationToken cancellationToken = default)
        {
            var html = EmailTemplateBuilder.BuildOrderConfirmationTemplate(model);
            var subject = $"[TuanTaiCMS] Xác nhận đơn hàng {model.OrderCode}";
            return await SendEmailAsync(model.CustomerEmail, model.CustomerName, subject, html, cancellationToken);
        }

        public async Task<bool> SendPaymentSuccessAsync(PaymentSuccessEmailModel model, CancellationToken cancellationToken = default)
        {
            var html = EmailTemplateBuilder.BuildPaymentSuccessTemplate(model);
            var subject = $"[TuanTaiCMS] Thanh toán thành công - Đơn hàng {model.OrderCode}";
            return await SendEmailAsync(model.CustomerEmail, model.CustomerName, subject, html, cancellationToken);
        }

        public async Task<bool> SendDeliverySuccessAsync(DeliverySuccessEmailModel model, CancellationToken cancellationToken = default)
        {
            var html = EmailTemplateBuilder.BuildDeliverySuccessTemplate(model);
            var subject = $"[TuanTaiCMS] Giao hàng thành công - Đơn hàng {model.OrderCode}";
            return await SendEmailAsync(model.CustomerEmail, model.CustomerName, subject, html, cancellationToken);
        }

        public async Task<bool> SendForgotPasswordAsync(ForgotPasswordEmailModel model, CancellationToken cancellationToken = default)
        {
            var html = EmailTemplateBuilder.BuildForgotPasswordTemplate(model);
            var subject = "[TuanTaiCMS] Đặt lại mật khẩu tài khoản";
            return await SendEmailAsync(model.CustomerEmail, model.CustomerName, subject, html, cancellationToken);
        }
    }
}
