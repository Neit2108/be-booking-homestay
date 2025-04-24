namespace HomestayBookingAPI.Models
{
    public class VNPayConfig
    {
        public string TmnCode { get; set; }
        public string HashSecret { get; set; }
        public string PaymentUrl { get; set; }
        public string ReturnUrl { get; set; }
        public string ApiUrl { get; set; }
        public string QrCreateUrl { get; set; }
        public string QrStatusUrl { get; set; }
    }
}
