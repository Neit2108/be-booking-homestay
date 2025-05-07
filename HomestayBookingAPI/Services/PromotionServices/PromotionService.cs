using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Promotion;
using HomestayBookingAPI.DTOs.Voucher;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
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

        public async Task<PromotionResponse> CreatePromotionAsync(PromotionRequest promoRequest, VoucherRequestForCreate voucherRequest)
        {
            if (promoRequest == null || voucherRequest == null)
            {
                throw new ArgumentNullException("Promotion or Voucher request cannot be null");
            }
            if (string.IsNullOrEmpty(promoRequest.Name) || string.IsNullOrEmpty(voucherRequest.Code))
            {
                throw new ArgumentException("Promotion name and voucher code cannot be empty");
            }
            if (promoRequest.StartDate >= promoRequest.EndDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }
            if (voucherRequest.Discount <= 0)
            {
                throw new ArgumentException("Discount must be greater than zero");
            }
            if (voucherRequest.MaxUsage <= 0)
            {
                throw new ArgumentException("Max usage must be greater than zero");
            }
            if (voucherRequest.UsageCount < 0)
            {
                throw new ArgumentException("Usage count cannot be negative");
            }
            if (promoRequest.PromotionType == PromotionType.Personal && promoRequest.PlaceId == null)
            {
                throw new ArgumentException("PlaceId must be provided for Personal promotions");
            }
            var voucher = new Voucher
            {
                Name = voucherRequest.Name,
                Code = voucherRequest.Code,
                UsageCount = voucherRequest.UsageCount,
                MaxUsage = voucherRequest.MaxUsage,
                Discount = voucherRequest.Discount,
                From = promoRequest.StartDate,
                To = promoRequest.EndDate
            };

            var promotion = new Promotion
            {
                Name = promoRequest.Name,
                Title = promoRequest.Title,
                Description = promoRequest.Description,
                StartDate = promoRequest.StartDate,
                EndDate = promoRequest.EndDate,
                Image = promoRequest.Image,
                PromotionType = promoRequest.PromotionType,
                Voucher = voucher
            };

            await _context.Vouchers.AddAsync(voucher);
            await _context.Promotions.AddAsync(promotion);

            if (promoRequest.PromotionType == PromotionType.Personal && promoRequest.PlaceId.HasValue)
            {
                var place = await _context.Places.FirstOrDefaultAsync(p => p.Id == promoRequest.PlaceId.Value);
                if (place == null)
                {
                    throw new KeyNotFoundException($"Place with ID {promoRequest.PlaceId.Value} not found");
                }

                if (promotion.Place == null)
                    promotion.Place = new List<Place>();

                promotion.Place.Add(place);
            }

            await _context.SaveChangesAsync();

            return new PromotionResponse
            {
                Name = promotion.Name,
                Title = promotion.Title,
                Description = promotion.Description,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                VoucherCode = voucher.Code,
                Discount = voucher.Discount,
                Image = promotion.Image,
                PromotionType = promotion.PromotionType,
                IsActive = DateTime.Now <= promotion.EndDate
            };
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
                VoucherCode = p.Voucher.Code ?? "deo co",
                Discount = p.Voucher.Discount,
                Image = p.Image,
                PromotionType = p.PromotionType,
                IsActive = DateTime.Now <= p.EndDate
            });
        }

        public async Task<PromotionResponse> GetPromotionByIdAsync(int id)
        {
            var promotion = await _context.Promotions.Include(p => p.Voucher)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (promotion == null)
            {
                throw new KeyNotFoundException($"Promotion with ID {id} not found");
            }
            return new PromotionResponse
            {
                Name = promotion.Name,
                Title = promotion.Title,
                Description = promotion.Description,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                VoucherCode = promotion.Voucher.Code ?? "deo co",
                Discount = promotion.Voucher.Discount,
                Image = promotion.Image,
                PromotionType = promotion.PromotionType,
                IsActive = DateTime.Now <= promotion.EndDate
            };
        }
    }
}
