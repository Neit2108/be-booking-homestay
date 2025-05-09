using System.ComponentModel.DataAnnotations;

namespace HomestayBookingAPI.DTOs.Wallet
{
    public class WalletDepositRequest
    {
        [Required]
        [Range(10000, double.MaxValue, ErrorMessage = "Số tiền nạp phải từ 10,000 VNĐ trở lên")]
        public double Amount { get; set; }

        [Required]
        public string ReturnUrl { get; set; }
        public string BankCode { get; set; }
    }
}
