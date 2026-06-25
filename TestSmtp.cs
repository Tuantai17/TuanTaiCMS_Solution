using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

class Program
{
    static async Task Main()
    {
        try {
            var smtpClient = new SmtpClient();
            smtpClient.CheckCertificateRevocation = false;
            smtpClient.Timeout = 10000;
            var sslOption = SecureSocketOptions.StartTls;
            await smtpClient.ConnectAsync("smtp.gmail.com", 587, sslOption);
            await smtpClient.AuthenticateAsync("", "");
            Console.WriteLine("SMTP Connection and Auth Successful!");
            await smtpClient.DisconnectAsync(true);
        } catch (Exception ex) {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
