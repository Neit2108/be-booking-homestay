using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.OwnerServices
{
    public interface IOwnerService
    {
        Task<ApplicationUser> RegisterOwner(RegisterOwnerRequest ownerForm, RegisterPlaceRequest placeForm);
    }
}
