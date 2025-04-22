using HomestayBookingAPI.DTOs.User;
using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.UserServices
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUserByID(string id);
        Task<string> GetUserRoleAsync(string id);
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    }
}
