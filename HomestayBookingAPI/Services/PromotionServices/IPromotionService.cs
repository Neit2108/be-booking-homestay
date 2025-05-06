using HomestayBookingAPI.DTOs.Promotion;
using HomestayBookingAPI.DTOs.Voucher;

namespace HomestayBookingAPI.Services.PromotionServices
{
    public interface IPromotionService
    {
        Task<IEnumerable<PromotionResponse>> GetAllPromotionsAsync();
        Task<PromotionResponse> GetPromotionByIdAsync(int id);
        Task<PromotionResponse> CreatePromotionAsync(PromotionRequest promoRequest, VoucherRequestForCreate voucherRequest);
    }
}
