using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomestayBookingAPI.Models
{
    public class TopRatePlaces
    {
        [Key]
        public int Id { get; set; }

        public int PlaceId { get; set; }

        [Range(0, 5)]
        public double Rating { get; set; }

        [Range(1, 5)]
        public int Rank { get; set; }

        [DataType(DataType.Date)]
        public DateTime LastUpdated { get; set; }

        [ForeignKey("PlaceId")]
        public Place Place { get; set; }
    }
}
