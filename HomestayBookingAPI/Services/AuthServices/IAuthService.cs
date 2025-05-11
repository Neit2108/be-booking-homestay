using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.Password;
using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.AuthService
{
    public interface IAuthService
    {
        Task<ApplicationUser> RegisterUser(RegisterDTO model);
        Task<LoginReponseDTO> LoginUser(LoginDTO model);
        Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    }
}
