using HomestayBookingAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace HomestayBookingAPI.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApplicationUser> GetUserByID(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user;
        }

    }
}
