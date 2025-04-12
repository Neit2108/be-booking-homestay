namespace HomestayBookingAPI.Models.Enum
{
    public enum NotificationType
    {
        ConfirmInfo, // gửi khách để xác nhận
        BookingRequest, // yêu cầu đặt chỗ
        BookingConfirmation, // xác nhận đặt chỗ
        BookingCancellation, // hủy đặt chỗ
        PaymentSuccess,
        PaymentFailure,
        ReviewReceived,
        ReviewResponse,
        AccountVerification,
        PasswordReset,
        GeneralUpdate
    }
}
