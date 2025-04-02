using System.ComponentModel.DataAnnotations;

namespace HomestayBookingAPI.Models
{
    public class LoginModel
    {
        [Required]
        public string EmailorUsername { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
