namespace HomestayBookingAPI.DTOs.Payment
{
    public class PaymentResponse
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string UserId { get; set; }
        public double Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public string PaymentUrl { get; set; }
        public string QrCodeUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
