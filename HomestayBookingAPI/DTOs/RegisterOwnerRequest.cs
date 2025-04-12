using System.ComponentModel.DataAnnotations;

namespace HomestayBookingAPI.DTOs
{
    public class RegisterOwnerRequest
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string IdentityCard { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string HomeAddress { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
