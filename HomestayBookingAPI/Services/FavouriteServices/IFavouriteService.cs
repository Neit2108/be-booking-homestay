using HomestayBookingAPI.DTOs.Favourite;

namespace HomestayBookingAPI.Services.FavouriteServices
{
    public interface IFavouriteService
    {
        Task<bool> AddFavouriteAsync(string userId, FavouriteRequest favouriteRequest);
        Task<IEnumerable<FavouriteResponse>> GetFavouritesByUserIdAsync(string userId);
        Task<bool> RemoveFavouriteAsync(string userId, FavouriteRequest favouriteRequest);
    }
}
