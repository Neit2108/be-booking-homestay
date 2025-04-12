namespace HomestayBookingAPI.Services.EmailServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string recipientEmail, string subject, string htmlMessage);

    }
}
