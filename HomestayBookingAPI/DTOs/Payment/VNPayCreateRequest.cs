namespace HomestayBookingAPI.DTOs.Payment
{
    public class VNPayCreateRequest
    {
        public int BookingId { get; set; }
        public string ReturnUrl { get; set; }
        public string OrderInfo { get; set; }
        public string OrderType { get; set; } = "270001";
        public string? BankCode { get; set; } // "VNPAYQR" hoặc "VNBANK" hoặc null
        public string Locale { get; set; } = "vn";
    }
}
