namespace HomestayBookingAPI.DTOs.Payment
{
    public class VNPayCreateRequest
    {
        public int BookingId { get; set; }
        public string ReturnUrl { get; set; }
        public string OrderInfo { get; set; }
        public string OrderType { get; set; } = "270001";

        // "VNPAYQR" - Thanh toán QR
        // "VNBANK" - Thanh toán qua thẻ ATM
        // "INTCARD" - Thanh toán qua thẻ quốc tế
        // null -hiển thị trang chọn
        public string? BankCode { get; set; } 
        public bool RequestDirectQR { get; set; } = false;
        public string Locale { get; set; } = "vn";
    }
}
