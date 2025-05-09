using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HomestayBookingAPI.Models.Enum;
using System.Text.Json.Serialization;

namespace HomestayBookingAPI.Models
{
    public class WalletTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WalletId { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        [EnumDataType(typeof(TransactionType))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TransactionType Type { get; set; }

        public string Description { get; set; }

        public int? BookingId { get; set; } // ID booking nếu là thanh toán

        public int? PaymentId { get; set; } // ID payment nếu là nạp tiền từ VNPAY

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("WalletId")]
        public virtual Wallet Wallet { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        [ForeignKey("PaymentId")]
        public virtual Payment Payment { get; set; }
    }
}
