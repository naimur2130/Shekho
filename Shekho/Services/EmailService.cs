using Microsoft.Extensions.Options;
using Shekho.Models;
using System.Net;
using System.Net.Mail;

namespace Shekho.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var client = new SmtpClient(_settings.Server, _settings.Port)
            {
                Credentials = new NetworkCredential(
                    _settings.Username,
                    _settings.Password
                ),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, "Shekho Platform"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}
