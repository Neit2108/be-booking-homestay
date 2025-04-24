namespace HomestayBookingAPI.DTOs.Payment
{
    public class VNPayCreateResponse
    {
        public int PaymentId { get; set; }
        public string PaymentUrl { get; set; }
        public string QrCodeUrl { get; set; }
        public string QrCodeBase64 { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
