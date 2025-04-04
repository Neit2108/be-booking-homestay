using HomestayBookingAPI.Models.Enum;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace HomestayBookingAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Column(TypeName = "text")]
        [StringLength(500)]
        public string FullName { get; set; }

        [Column(TypeName = "text")]
        [StringLength(500)]
        public string? HomeAddress { get; set; }
      
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Column(TypeName = "text")]
        public string? AvatarUrl { get; set; }

        [EnumDataType(typeof(Gender))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender Gender { get; set; }

        [Column(TypeName = "text")]
        [StringLength(1000)]
        public string? Bio { get; set; }

    }
}
