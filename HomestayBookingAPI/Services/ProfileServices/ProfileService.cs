using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.UserServices;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace HomestayBookingAPI.Services.ProfileServices
{
    public class ProfileService : IProfileService
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        public async Task<bool> UpdateProfileAsync(string id, ProfileDTO model)
        {
            var user = await _userService.GetUserByID(id);

            if (user == null)
            {
                return false;
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.HomeAddress = model.Address;
            //user.AvatarUrl = model.AvatarUrl;
            user.BirthDate = model.BirthDate;
            user.Gender = model.Gender;
            user.Bio = model.Bio;

            var res =  await _userManager.UpdateAsync(user);
            return res.Succeeded;
        }
    }
}
