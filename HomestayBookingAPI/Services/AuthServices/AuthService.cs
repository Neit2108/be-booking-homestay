using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.Password;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.JwtServices;
using HomestayBookingAPI.Services.WalletServices;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace HomestayBookingAPI.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWalletService _walletService;
        private readonly IJwtService _jwtService;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IJwtService jwtService, IWalletService walletService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _walletService = walletService;
        }

        public async Task<ApplicationUser> RegisterUser(RegisterDTO model)
        {
            var user = new ApplicationUser
            {
                FullName = model.FullName,
                IdentityCard = model.IdentityCard,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                HomeAddress = model.HomeAddress,
                UserName = model.Username,
                CreateAt = DateTime.UtcNow,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Tenant");
                await _signInManager.SignInAsync(user, isPersistent: false);

                await _walletService.GetOrCreateWalletAsync(user.Id);
                return user;
            }
            return null;
        }

        public async Task<LoginReponseDTO> LoginUser(LoginDTO model)
        {
            bool isEmail = Regex.IsMatch(model.EmailorUsername, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            var user = isEmail
                ? await _userManager.FindByEmailAsync(model.EmailorUsername)
                : await _userManager.FindByNameAsync(model.EmailorUsername);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return null;
            }

            string token =  _jwtService.GenerateSecurityToken(user);
            return new LoginReponseDTO
            {
                Token = token,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl
            };
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return false;
            }
            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (result.Succeeded)
            {
                user.PasswordChangeAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                return true;
            }
            return false;
        }
    }
}
