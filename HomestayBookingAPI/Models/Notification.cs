using HomestayBookingAPI.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HomestayBookingAPI.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RecipientId { get; set; } // ng nhận 

        [Required]
        public string SenderId { get; set; } // ng gửi

        public int? BookingId { get; set; }

        public string JobId { get; set; } // id của job trong hangfire (nếu có) để xóa job khi đã gửi email thành công

        [Required]
        [EnumDataType(typeof(NotificationType))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NotificationType Type { get; set; } 

        [Required]

        public string Title { get; set; }
        
        [Required]
        public string Message { get; set; }

        [Required]
        public bool IsRead { get; set; } = false;

        [Required]
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        [Required]
        public string Url { get; set; } // url tới các trang ()

        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; } // ng gửi

        [ForeignKey("RecipientId")]
        public virtual ApplicationUser Recipient { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } 
    }
}
