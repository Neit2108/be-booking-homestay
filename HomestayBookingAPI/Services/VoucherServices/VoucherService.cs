using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Voucher;
using HomestayBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HomestayBookingAPI.Services.VoucherServices
{
    public class VoucherService : IVoucherService
    {
        private readonly ApplicationDbContext _context;

        public VoucherService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<double> ApplyVoucherAsync(string clientVoucher, double price)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == clientVoucher);
            if (voucher == null)
            {
                return price;
            }
            if (voucher.MaxUsage < voucher.UsageCount)
            {
                return price;
            }
            if (voucher.From > voucher.To)
            {
                return price;
            }
            double discount = price * (voucher.Discount / 100);
            voucher.UsageCount++;
            await _context.SaveChangesAsync();
            return price - discount;
        }

        public async Task<VoucherResponse> CheckVoucherAvailable(string voucher)
        {
            var checkedVoucher = await GetVoucherByCode(voucher);
            if (checkedVoucher == null)
            {
                return null;
            }

            var start = checkedVoucher.From;
            var end = checkedVoucher.To;
            var now = DateTime.Now;
            bool isInRange = now >= start && now <= end;

            if (!isInRange)
            {
                return null;
            }

            return new VoucherResponse
            {
                Code = voucher,
                Discount = checkedVoucher.Discount
            };

        }

        public async Task<Voucher> GetVoucherByCode(string voucher)
        {
            if (string.IsNullOrEmpty(voucher))
            {
                return null; 
            }

            return await _context.Vouchers.FirstOrDefaultAsync(v => v.Code.ToLower() == voucher.ToLower());
        }
    }
}
