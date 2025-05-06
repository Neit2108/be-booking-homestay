using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.DTOs.Promotion
{
    public class PromotionRequest
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Image { get; set; }
        
        public PromotionType PromotionType { get; set; }
        public int? PlaceId { get; set; } // nếu personal
        //public string VoucherCode { get; set; }
        //public string Discount { get; set; }
        //public int UsageCount { get; set; }
        //public int MaxUsage { get; set; }
    }
}
