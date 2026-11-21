using System.Net;
using System.Net.Mail;
using DatLichKhamBenh.Services.Interfaces;

namespace DatLichKhamBenh.Services.Impl
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string toEmail, string subject, string body)
        {
            var host = _configuration["Email:SmtpHost"];
            var portText = _configuration["Email:SmtpPort"];
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var from = _configuration["Email:From"] ?? username;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portText)
                || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(from))
            {
                _logger.LogWarning("SMTP chưa được cấu hình. Không gửi email tới {Email}.", toEmail);
                return false;
            }

            if (!int.TryParse(portText, out var port))
            {
                _logger.LogError("Email:SmtpPort không hợp lệ.");
                return false;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(from),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(username, password)
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
