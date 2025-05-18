using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.Password;
using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.AuthService
{
    public interface IAuthService
    {
        Task<ApplicationUser> RegisterUser(RegisterDTO model);
        Task<LoginReponseDTO> LoginUser(LoginDTO model);
        Task<LoginReponseDTO> LoginUser2FA(string userId, string otp);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> EnableTwoFactorAsync(ApplicationUser user, string token);
        Task<bool> SendTwoFactorEnableOtpAsync(ApplicationUser user);
        Task<bool> DisableTwoFactorAsync(ApplicationUser user);

    }
}
