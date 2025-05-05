using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.DTOs.Promotion
{
    public class PromotionResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int VoucherId { get; set; }
        public string? Image { get; set; }
        public PromotionType PromotionType { get; set; } 
    }
}
