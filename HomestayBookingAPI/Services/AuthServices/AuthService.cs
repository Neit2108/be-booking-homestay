using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace HomestayBookingAPI.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, JwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        public async Task<ApplicationUser> RegisterUser(RegisterDTO model)
        {
            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                HomeAddress = model.HomeAddress,
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Tenant");
                await _signInManager.SignInAsync(user, isPersistent: false);
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
    }
}
