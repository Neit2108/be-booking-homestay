using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.Models
{
    public class Promotion
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "text")]
        public string Name { get; set; }
        [Column(TypeName = "text")]
        [Required]
        public string Title { get; set; }
        [Column(TypeName = "text")]
        public string Description { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public string? Image { get; set; }
        [Required]
        [ForeignKey("VoucherId")]
        public virtual Voucher Voucher { get; set; }
        [Required]
        [EnumDataType(typeof(PromotionType))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PromotionType PromotionType { get; set; }
        public virtual ICollection<Place>? Place { get; set; } // nếu personal
    }
}
