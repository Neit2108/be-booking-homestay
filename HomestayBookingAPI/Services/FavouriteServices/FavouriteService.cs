using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Favourite;
using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.FavouriteServices
{
    public class FavouriteService : IFavouriteService
    {
        private readonly ApplicationDbContext _context;

        public FavouriteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FavouriteResponse> AddFavouriteAsync(FavouriteRequest favouriteRequest)
        {
            var favourite = new Favourite
            {
                UserId = favouriteRequest.UserId,
                PlaceId = favouriteRequest.PlaceId
            };

            await _context.Favourites.AddAsync(favourite);
            await _context.SaveChangesAsync();

            var favouriteResponse = new FavouriteResponse
            {
                UserId = favourite.UserId,
                PlaceId = favourite.PlaceId
            };

            return favouriteResponse;
        }
    }
}
