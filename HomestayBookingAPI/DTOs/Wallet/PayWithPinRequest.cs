using System.ComponentModel.DataAnnotations;

namespace HomestayBookingAPI.DTOs.Wallet
{
    public class PayWithPinRequest
    {
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Mã PIN là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "PIN phải có đúng 6 số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "PIN phải chứa đúng 6 chữ số")]
        public string Pin { get; set; }
    }
}
