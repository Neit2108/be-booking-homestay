﻿using HomestayBookingAPI.DTOs.Notify;
using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.NotifyServices
{
    public interface INotifyService
    {
        Task CreateNewBookingNotificationAsync(Booking booking, bool sendEmail = true); // Dungf khi 1 booking mới được tạo 
        Task NotifyBookingStatusChangeAsync(int bookingId, bool isAccepted, string rejectReason = "Không xác định"); // Dùng khi trạng thái booking thay đổi
        Task UpdateNotificationStatusAsync();
        Task CreatePaymentSuccessNotificationAsync(Booking booking);
        Task CreatePaymentFailureNotificationAsync(Booking booking);
        Task CreateWalletTransactionNotificationAsync(WalletTransaction transaction);
        Task CreateForgotPasswordNotificationAsync(string email, string newPassword);
        Task<IEnumerable<NotifyResponse>> GetAllNotifyByUserId(string userId);

    }
}
