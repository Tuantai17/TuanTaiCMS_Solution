using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

class Program
{
    static async Task Main(string[] args)
    {
        var smtpServer = "smtp.gmail.com";
        var port = 587;
        var senderEmail = "cynex672@gmail.com";
        var password = "YOUR_APP_PASSWORD_HERE"; // I don't know the password, I can't test it directly unless I read appsettings.json
        Console.WriteLine("Test starting...");
    }
}
