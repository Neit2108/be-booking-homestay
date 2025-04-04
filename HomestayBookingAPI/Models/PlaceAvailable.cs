using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomestayBookingAPI.Models
{
    public class PlaceAvailable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PlaceId { get; set; }

        //[Required]
        //public int RoomId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        [Required]
        public double Price { get; set; }

        [ForeignKey("PlaceId")]
        public virtual Place Place { get; set; }

    }
}
