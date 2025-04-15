using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.NotifyServices
{
    public interface INotifyService
    {
        Task CreateNewBookingNotificationAsync(Booking booking, bool sendEmail = true); // Dungf khi 1 booking mới được tạo 
        Task NotifyBookingStatusChangeAsync(int bookingId, bool isAccepted, string rejectReason = "Không xác định"); // Dùng khi trạng thái booking thay đổi
        Task UpdateNotificationStatusAsync();

    }
}
