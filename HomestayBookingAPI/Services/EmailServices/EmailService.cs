
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using System.IO;

namespace HomestayBookingAPI.Services.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(recipientEmail))
            {
                _logger.LogError("Recipient email is null or empty.");
                throw new ArgumentNullException(nameof(recipientEmail));
            }

            if (string.IsNullOrEmpty(htmlMessage))
            {
                _logger.LogError("HTML message is null or empty.");
                throw new ArgumentNullException(nameof(htmlMessage));
            }

            var message = new MimeMessage();
            try
            {
                message.From.Add(new MailboxAddress("HomiesStay", _config["Smtp:SenderEmail"]));
                message.To.Add(new MailboxAddress("", recipientEmail));
                message.Subject = subject;

                message.Body = new TextPart("html")
                {
                    Text = htmlMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create MimeMessage for recipient {RecipientEmail}.", recipientEmail);
                throw;
            }

            using var client = new SmtpClient();
            bool emailSent = false;
            string emailsavefile = null;

            try
            {
                _logger.LogInformation("Connecting to SMTP server {SmtpHost}:{SmtpPort} for recipient {RecipientEmail}.",
                    _config["Smtp:Host"], _config["Smtp:Port"], recipientEmail);

                await client.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"]);

                _logger.LogInformation("Sending email to {RecipientEmail}.", recipientEmail);
                await client.SendAsync(message);

                emailSent = true;
                _logger.LogInformation("Email sent successfully to {RecipientEmail}.", recipientEmail);
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError(ex, "SMTP command error while sending email to {RecipientEmail}. Status: {StatusCode}",
                    recipientEmail, ex.StatusCode);

                // Lưu email vào thư mục mailssave
                Directory.CreateDirectory("mailssave");
                emailsavefile = Path.Combine("mailssave", $"{Guid.NewGuid()}.eml");
                await message.WriteToAsync(emailsavefile);
                _logger.LogInformation("Email failed to send, saved to {EmailSaveFile}.", emailsavefile);
            }
            catch (SmtpProtocolException ex)
            {
                _logger.LogError(ex, "SMTP protocol error while sending email to {RecipientEmail}.", recipientEmail);

                Directory.CreateDirectory("mailssave");
                emailsavefile = Path.Combine("mailssave", $"{Guid.NewGuid()}.eml");
                await message.WriteToAsync(emailsavefile);
                _logger.LogInformation("Email failed to send, saved to {EmailSaveFile}.", emailsavefile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending email to {RecipientEmail}.", recipientEmail);

                Directory.CreateDirectory("mailssave");
                emailsavefile = Path.Combine("mailssave", $"{ Guid.NewGuid()}.eml");
                await message.WriteToAsync(emailsavefile);
                _logger.LogInformation("Email failed to send, saved to {EmailSaveFile}.", emailsavefile);
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                    _logger.LogInformation("Disconnected from SMTP server for recipient {RecipientEmail}.", recipientEmail);
                }

                if (!emailSent && emailsavefile == null)
                {
                    _logger.LogWarning("Email to {RecipientEmail} was not sent and not saved.", recipientEmail);
                }
            }
        }
    }
}
