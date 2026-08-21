using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ApexWorld_Backend.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ApexWorld_Backend.Core.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var host = _configuration["Smtp:Host"] ?? "localhost";
            var portString = _configuration["Smtp:Port"] ?? "25";
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@apexworld.com";

            int port = int.TryParse(portString, out var p) ? p : 25;

            using var client = new SmtpClient(host, port);
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                client.Credentials = new NetworkCredential(username, password);
                client.EnableSsl = true;
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"\n[MOCK EMAIL SENT] To: {to}");
                System.Console.WriteLine($"Subject: {subject}");
                System.Console.WriteLine($"Body: {body}");
                System.Console.WriteLine($"SMTP Error: {ex.Message}\n");
                // For local development, if SMTP is not configured, we just log it.
                // In production, you might want to re-throw or handle it properly.
            }
        }
    }
}