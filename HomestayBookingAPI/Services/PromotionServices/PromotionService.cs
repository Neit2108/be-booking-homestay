using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Promotion;
using Microsoft.EntityFrameworkCore;

namespace HomestayBookingAPI.Services.PromotionServices
{
    public class PromotionService : IPromotionService
    {
        private readonly ApplicationDbContext _context;
        public PromotionService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<PromotionResponse>> GetAllPromotionsAsync()
        {
            var promotions = await _context.Promotions.Include(p => p.Voucher).ToListAsync();
            return promotions.Select(p => new PromotionResponse
            {
                Name = p.Name,
                Title = p.Title,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                VoucherId = p.Voucher.Id,
                Image = p.Image,
                PromotionType = p.PromotionType
            });
        }
    }
}
