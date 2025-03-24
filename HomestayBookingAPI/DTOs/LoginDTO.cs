using System.ComponentModel.DataAnnotations;

namespace HomestayBookingAPI.DTOs
{
    public class LoginDTO
    {
        [Required]
        public string EmailorUsername { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
