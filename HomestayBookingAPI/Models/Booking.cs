using HomestayBookingAPI.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HomestayBookingAPI.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int PlaceId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int NumberOfGuests { get; set; }

        [Required]
        public double TotalPrice { get; set; }

        [Required]
        [EnumDataType(typeof(BookingStatus))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Required]
        [EnumDataType(typeof(PaymentStatus))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("PlaceId")]
        public virtual Place Place { get; set; }
    }
}
