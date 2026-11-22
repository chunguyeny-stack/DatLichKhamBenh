using System.Net;
using System.Net.Mail;
using DatLichKhamBenh.Configuration;
using DatLichKhamBenh.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DatLichKhamBenh.Services.Impl
{
    public class EmailService : IEmailService
    {
        private readonly MediSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<MediSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host)
                || _settings.Port <= 0
                || string.IsNullOrWhiteSpace(_settings.SenderEmail)
                || string.IsNullOrWhiteSpace(_settings.Password))
            {
                _logger.LogWarning("SMTP chưa được cấu hình. Không gửi email tới {Email}.", toEmail);
                return false;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    Credentials = new NetworkCredential(_settings.SenderEmail, _settings.Password)
                };

                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể gửi email tới {Email}.", toEmail);
                return false;
            }
        }
    }
}
