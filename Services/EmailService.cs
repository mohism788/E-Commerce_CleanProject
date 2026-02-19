using E_Commerce.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace E_Commerce.Services
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

        public async Task SendEmailAsync(string to, string subject, string body, string replyTo = null)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(smtpSettings["SenderName"], smtpSettings["SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            if (!string.IsNullOrEmpty(replyTo))
            {
                email.ReplyTo.Add(MailboxAddress.Parse(replyTo));
            }

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                _logger.LogInformation("Connecting to SMTP server {Server}:465...", smtpSettings["Server"]);
                await smtp.ConnectAsync(
                    smtpSettings["Server"], 
                    465, 
                    SecureSocketOptions.SslOnConnect);

                _logger.LogInformation("Authenticating as {User}...", smtpSettings["Username"]);
                await smtp.AuthenticateAsync(smtpSettings["Username"], smtpSettings["Password"]);
                
                _logger.LogInformation("Sending email to {To}...", to);
                await smtp.SendAsync(email);
                
                await smtp.DisconnectAsync(true);
                _logger.LogInformation("Email sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP Error: {Message}", ex.Message);
                throw;
            }
        }
    }
}
