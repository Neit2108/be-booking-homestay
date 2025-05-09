using System.ComponentModel.DataAnnotations;

namespace HomestayBookingAPI.DTOs.Wallet
{
    public class SetPinRequest
    {
        [Required]
        [StringLength(6, MinimumLength = 4, ErrorMessage = "PIN phải có từ 4-6 số")]
        [RegularExpression(@"^\d+$", ErrorMessage = "PIN chỉ được chứa các chữ số")]
        public string Pin { get; set; }
    }
}
