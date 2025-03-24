using HomestayBookingAPI.DTOs;

namespace HomestayBookingAPI.Services.ProfileServices
{
    public interface IProfileService
    {
        Task<bool> UpdateProfileAsync(string id, ProfileDTO model);
    }
}
