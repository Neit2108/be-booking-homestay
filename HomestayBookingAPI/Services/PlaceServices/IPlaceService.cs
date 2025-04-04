using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.PlaceServices
{
    public interface IPlaceService
    {
        Task<List<PlaceDTO>> GetTopRatePlace(int limit);

        Task<Place> AddPlaceAsync(Place place);

        Task<PlaceDTO> GetPlaceByID(int id);

        Task<List<PlaceDTO>> GetAllPlacesAsync();

        Task<List<PlaceDTO>> GetSameCategoryPlaces(int id);

        Task<List<string>> UploadImagePlaceAsync(int placeId, List<IFormFile> files);
    }
}
