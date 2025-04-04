using HomestayBookingAPI.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HomestayBookingAPI.DTOs
{
    public class ProfileDTO
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [EnumDataType(typeof(Gender))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender Gender { get; set; }
        public string? Bio { get; set; }
    }
}
