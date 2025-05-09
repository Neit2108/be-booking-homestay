namespace HomestayBookingAPI.DTOs.Wallet
{
    public class WalletResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public double Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
