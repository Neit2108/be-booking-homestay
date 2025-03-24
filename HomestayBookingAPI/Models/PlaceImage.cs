using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomestayBookingAPI.Models
{
    public class PlaceImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        public int PlaceId { get; set; }

        [ForeignKey("PlaceId")]
        public Place Place { get; set; }
    }
}
