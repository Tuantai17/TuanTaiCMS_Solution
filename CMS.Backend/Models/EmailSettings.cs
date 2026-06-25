namespace CMS.Backend.Models
{
    /// <summary>
    /// Cau hinh SMTP cho dich vu gui email.
    /// Gia tri duoc doc tu appsettings.json va User Secrets.
    /// </summary>
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string SenderName { get; set; } = "TuanTaiCMS Shop";
        public string SenderEmail { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public string FrontendBaseUrl { get; set; } = "http://localhost:3000";
    }
}
