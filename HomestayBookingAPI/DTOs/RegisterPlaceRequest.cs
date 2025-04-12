using HomestayBookingAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace HomestayBookingAPI.DTOs
{
    public class RegisterPlaceRequest
    {
        [Required]
        public string PlaceName { get; set; }

        [Required]
        public string PlaceAddress { get; set; }

        [Required]
        public double PlacePrice { get; set; }

        [Required]
        public List<IFormFile> PlaceImages { get; set; }

        //public List<string?> PlaceDocuments { get; set; }

        [Required]
        public string PlaceDescription { get; set; }

    }
}
