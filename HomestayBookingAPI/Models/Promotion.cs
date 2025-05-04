using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Voucher Voucher { get; set; }
        public PromotionType PromotionType { get; set; }
        public bool IsGlobal { get; set; }
    }
}
