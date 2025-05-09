using HomestayBookingAPI.DTOs.Payment;

namespace HomestayBookingAPI.Services.PaymentServices
{
    public interface IVNPayService
    {
        Task<VNPayCreateResponse> CreatePaymentUrlAsync(VNPayCreateRequest request, string ipAddress);
        Task<PaymentResponse> ProcessPaymentCallbackAsync(Dictionary<string, string> vnpayData);
        Task<PaymentResponse> GetPaymentByIdAsync(int paymentId);
        Task<IEnumerable<PaymentResponse>> GetPaymentsByBookingIdAsync(int bookingId);
        Task<IEnumerable<PaymentResponse>> GetPaymentsByUserIdAsync(string userId);
        Task<VNPayCreateResponse> CreateGenericPaymentAsync(GenericPaymentRequest request, string userId, string ipAddress);
    }
}
