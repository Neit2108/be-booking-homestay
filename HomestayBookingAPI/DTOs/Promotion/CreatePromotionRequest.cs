using HomestayBookingAPI.DTOs.Voucher;

namespace HomestayBookingAPI.DTOs.Promotion
{
    public class CreatePromotionRequest
    {
        public PromotionRequest Promotion { get; set; }
        public VoucherRequestForCreate Voucher { get; set; }
    }
}
