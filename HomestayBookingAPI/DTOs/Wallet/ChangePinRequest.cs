using System.ComponentModel.DataAnnotations;

namespace HomestayBookingAPI.DTOs.Wallet
{
    public class ChangePinRequest
    {
        [Required(ErrorMessage = "Mã PIN hiện tại là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "PIN phải có đúng 6 số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "PIN phải chứa đúng 6 chữ số")]
        public string OldPin { get; set; }

        [Required(ErrorMessage = "Mã PIN mới là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "PIN phải có đúng 6 số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "PIN phải chứa đúng 6 chữ số")]
        public string NewPin { get; set; }
    }
}