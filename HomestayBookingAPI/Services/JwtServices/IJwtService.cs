using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.JwtServices
{
    public interface IJwtService
    {
        string GenerateSecurityToken(ApplicationUser user);
        //string GenerateEmailConfirmationToken(ApplicationUser user, string role, int bookingId = null);
        string GenerateActionToken(string userId, string action, int referenceId, string role);
    }
}
