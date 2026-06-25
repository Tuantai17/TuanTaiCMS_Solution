using CMS.Backend.Models;

namespace CMS.Backend.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string htmlBody, CancellationToken cancellationToken = default);
        Task<bool> SendOrderConfirmationAsync(OrderEmailModel model, CancellationToken cancellationToken = default);
        Task<bool> SendPaymentSuccessAsync(PaymentSuccessEmailModel model, CancellationToken cancellationToken = default);
        Task<bool> SendDeliverySuccessAsync(DeliverySuccessEmailModel model, CancellationToken cancellationToken = default);
        Task<bool> SendForgotPasswordAsync(ForgotPasswordEmailModel model, CancellationToken cancellationToken = default);
    }
}
