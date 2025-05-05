using HomestayBookingAPI.DTOs.Voucher;
using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.VoucherServices
{
    public interface IVoucherService
    {
        Task<VoucherResponse> CheckVoucherAvailable(string voucher);
        Task<Voucher> GetVoucherByCode(string voucher);
        Task<double> ApplyVoucherAsync(string clientVoucher, double price);
        Task<Voucher> CreateVoucherDefaultAsync(VoucherRequestForCreate voucherRequestForCreate);
    }
}
