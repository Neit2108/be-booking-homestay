using HomestayBookingAPI.DTOs.Promotion;

namespace HomestayBookingAPI.Services.PromotionServices
{
    public interface IPromotionService
    {
        Task<IEnumerable<PromotionResponse>> GetAllPromotionsAsync();
        
    }
}
