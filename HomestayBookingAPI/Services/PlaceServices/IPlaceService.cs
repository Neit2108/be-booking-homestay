using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.Place;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.Services.PlaceServices
{
    public interface IPlaceService
    {
        Task<List<PlaceDTO>> GetTopRatePlace(int limit, string userId);

        Task<PlaceResponse> AddPlaceAsync(PlaceRequest placeRequest);

        Task<PlaceDTO> GetPlaceByID(int id, string userId);

        Task<List<PlaceDTO>> GetAllPlacesAsync(string userId);

        Task<List<PlaceDTO>> GetAllPlacesOfLandlord(string landlordId);

        Task<List<PlaceDTO>> GetSameCategoryPlaces(int id, string userId);

        Task<List<string>> UploadImagePlaceAsync(int placeId, List<IFormFile> files);
        Task<bool> UpdatePlaceStatusAsync(UpdatePlaceStatusRequest request);
    }
}
