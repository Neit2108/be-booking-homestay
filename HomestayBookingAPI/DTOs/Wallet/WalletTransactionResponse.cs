namespace HomestayBookingAPI.DTOs.Wallet
{
    public class WalletTransactionResponse
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public double Amount { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public int? BookingId { get; set; }
        public int? PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
