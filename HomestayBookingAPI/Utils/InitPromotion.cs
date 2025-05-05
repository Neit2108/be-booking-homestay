using System.Threading;
using HomestayBookingAPI.Data;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.VoucherServices;
using Microsoft.EntityFrameworkCore;

namespace HomestayBookingAPI.Utils
{
    public static class InitPromotion
    {
        //private readonly IVoucherService _voucherService;
        //private readonly ApplicationDbContext _context;

        //public InitPromotion(IVoucherService voucherService)
        //{
        //    _voucherService = voucherService;
        //}
        public static async Task InitPromotionAsync(ApplicationDbContext _context, IVoucherService _voucherService)
        {
            if (!await _context.Vouchers.AnyAsync(v =>
                    v.Code == "SUMMER2025" ||
                    v.Code == "GROUP2025" ||
                    v.Code == "LONGSTAY"
                    ))
            {
                var currentDate = DateTime.Now;
                var maxDate = new DateTime(2099, 12, 31);

                var summerVoucher = new Voucher
                {
                    Name = "Voucher chào hè",
                    Code = "SUMMER2025",
                    UsageCount = 0,
                    MaxUsage = int.MaxValue,
                    Discount = 30,
                    From = currentDate,
                    To = maxDate
                };

                var groupVoucher = new Voucher
                {
                    Name = "Voucher nhóm",
                    Code = "GROUP2025",
                    UsageCount = 0,
                    MaxUsage = int.MaxValue,
                    Discount = 15,
                    From = currentDate,
                    To = maxDate
                };

                var longStayVoucher = new Voucher
                {
                    Name = "Voucher kì nghỉ dài",
                    Code = "LONGSTAY",
                    UsageCount = 0,
                    MaxUsage = int.MaxValue,
                    Discount = 25,
                    From = currentDate,
                    To = maxDate
                };

                await _context.Vouchers.AddRangeAsync(summerVoucher, groupVoucher, longStayVoucher);

                var summerPromotion = new Promotion
                {
                    Name = "Chào hè cùng Homies",
                    Title = "Khuyến mãi kì hè",
                    Description = "Giảm giá 30% cho tất cả các đặt phòng trong mùa hè 2025",
                    StartDate = currentDate,
                    EndDate = maxDate,
                    Voucher = summerVoucher,
                    Image = "https://res.cloudinary.com/dbswzktwo/image/upload/v1746276903/avatars/q5gujgw7dxldljhnca3i.jpg",
                    PromotionType = Models.Enum.PromotionType.Global,
                };

                var groupPromotion = new Promotion
                {
                    Name = "Ưu đãi nhóm",
                    Title = "Đi cùng bạn bè, tiết kiệm hơn",
                    Description = "Đặt phòng cho nhóm từ 5 người trở lên, nhận ngay ưu đãi 15% tổng hóa đơn và dịch vụ đưa đón miễn phí.",
                    StartDate = currentDate,
                    EndDate = maxDate,
                    Voucher = groupVoucher,
                    Image = "https://res.cloudinary.com/dbswzktwo/image/upload/v1746276970/avatars/ueaec3rxczixkqex8xen.jpg",
                    PromotionType = Models.Enum.PromotionType.Global,
                };

                var longStayPromotion = new Promotion
                {
                    Name = "Kì nghỉ dài hạn",
                    Title = "Ở càng lâu, giá càng tốt",
                    Description = "Đặt phòng từ 7 đêm trở lên, nhận ngay ưu đãi 25% tổng hóa đơn và dịch vụ đưa đón miễn phí.",
                    StartDate = currentDate,
                    EndDate = maxDate,
                    Voucher = longStayVoucher,
                    PromotionType = Models.Enum.PromotionType.Global,
                };

                await _context.Promotions.AddRangeAsync(summerPromotion, groupPromotion, longStayPromotion);
                await _context.SaveChangesAsync();
            }
        }
    }
}
