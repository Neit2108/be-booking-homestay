using System.ComponentModel.DataAnnotations;
using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.DTOs.Payment
{
    public class GenericPaymentRequest
    {
        [Required]
        public double Amount { get; set; }

        [Required]
        public string ReturnUrl { get; set; }

        [Required]
        public PaymentPurpose Purpose { get; set; }

        // ID booking (nếu là thanh toán booking)
        public int? BookingId { get; set; }

        // Thông tin mô tả giao dịch
        public string OrderInfo { get; set; }

        // Các tùy chọn VNPay
        public string OrderType { get; set; } = "270001";
        public string? BankCode { get; set; }
        public string Locale { get; set; } = "vn";
    }
}