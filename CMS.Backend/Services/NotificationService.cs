using CMS.Data;
using CMS.Data.Entities;

namespace CMS.Backend.Services
{
    public interface INotificationService
    {
        Task CreateAsync(string title, string message, string notificationType, string? referenceType = null, int? referenceId = null, int? targetUserId = null, int? targetCustomerId = null);
        Task CreateForAllAdminsAsync(string title, string message, string notificationType, string? referenceType = null, int? referenceId = null);
        Task CreateForCustomerAsync(string title, string message, string notificationType, int customerId, string? referenceType = null, int? referenceId = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateAsync(string title, string message, string notificationType, string? referenceType = null, int? referenceId = null, int? targetUserId = null, int? targetCustomerId = null)
        {
            try
            {
                var notification = new Notification
                {
                    Title = title,
                    Message = message,
                    NotificationType = notificationType,
                    ReferenceType = referenceType,
                    ReferenceId = referenceId,
                    TargetUserId = targetUserId,
                    TargetCustomerId = targetCustomerId,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loi tao thong bao: {Title}", title);
            }
        }

        public async Task CreateForAllAdminsAsync(string title, string message, string notificationType, string? referenceType = null, int? referenceId = null)
        {
            // Tao thong bao voi TargetUserId = null (tat ca admin deu thay)
            await CreateAsync(title, message, notificationType, referenceType, referenceId, null, null);
        }

        public async Task CreateForCustomerAsync(string title, string message, string notificationType, int customerId, string? referenceType = null, int? referenceId = null)
        {
            await CreateAsync(title, message, notificationType, referenceType, referenceId, null, customerId);
        }
    }
}
