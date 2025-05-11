using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.OwnerServices
{
    public interface IOwnerService
    {
        Task<ApplicationUser> RegisterOwner(RegisterOwnerRequest ownerForm, RegisterPlaceRequest placeForm);
        // Rút tiền về ví
        //Task<bool> WithdrawMoney(string userId, double amount);
        Task<double> GetSales(string landlordId);
    }
}
