using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.NotifyServices
{
    public interface INotifyService
    {
        Task CreateNewBookingNotificationAsync(Booking booking, bool sendEmail); // Dungf khi 1 booking mới được tạo
        Task SendBookingEmailAsync(int bookingId); 
    }
}
