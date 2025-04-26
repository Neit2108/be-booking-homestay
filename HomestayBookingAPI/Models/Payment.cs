using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomestayBookingAPI.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public int BookingId { get; set; }
        public double Amount { get; set; }
        public string PaymentMethod { get; set; } = "VNPAY";
        public string Status { get; set; } = "Pending";
        public string? TransactionId { get; set; }
        public string? PaymentUrl { get; set; }
        public string? QrCodeUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaymentDate { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }
    }
}
