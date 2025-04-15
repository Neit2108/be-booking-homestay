using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.UserServices
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUserByID(string id);
        Task<string> GetUserRoleAsync(string id);
    }
}
