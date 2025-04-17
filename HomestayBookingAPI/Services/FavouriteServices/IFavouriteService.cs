using HomestayBookingAPI.DTOs.Favourite;

namespace HomestayBookingAPI.Services.FavouriteServices
{
    public interface IFavouriteService
    {
        Task<FavouriteResponse> AddFavouriteAsync(FavouriteRequest favouriteRequest);
    }
}
