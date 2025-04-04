using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomestayBookingAPI.Models
{
    public class Place
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Column(TypeName = "text")]
        public string Name { get; set; }

        [Column(TypeName = "text")]
        [StringLength(500)]
        [Required]
        public string Address { get; set; }

        [Range(0, 5)]
        public double Rating { get; set; }

        [Range(0, int.MaxValue)]
        public int NumOfRating { get; set; }

        [Column(TypeName = "text")]
        [StringLength(50)]
        public string Category { get; set; }

        [Column(TypeName = "text")]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public int MaxGuests { get; set; }

        public List<PlaceImage> Images { get; set; } = new List<PlaceImage>(); // 1 - n với PlaceImage
    }
}
