using HomestayBookingAPI.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

        [Required]
        [EnumDataType(typeof(PlaceStatus))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlaceStatus Status { get; set; } = PlaceStatus.Pending;

        [Required]
        public string OwnerId { get; set; } 

        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; } // FK đến User

        public List<PlaceImage> Images { get; set; } = new List<PlaceImage>(); // 1 - n với PlaceImage
        public List<Favourite> Favourites { get; set; } = new List<Favourite>(); // 1 - n với Favourite
    }
}
