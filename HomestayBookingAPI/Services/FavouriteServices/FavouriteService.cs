using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Favourite;
using HomestayBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HomestayBookingAPI.Services.FavouriteServices
{
    public class FavouriteService : IFavouriteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FavouriteService> _logger;

        public FavouriteService(ApplicationDbContext context, ILogger<FavouriteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddFavouriteAsync(string userId, FavouriteRequest favouriteRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
                }
                if (favouriteRequest == null)
                {
                    throw new ArgumentNullException(nameof(favouriteRequest), "Favourite request cannot be null.");
                }
                var favourite = new Favourite
                {
                    UserId = userId,
                    PlaceId = favouriteRequest.PlaceId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var place = await _context.Places.FindAsync(favouriteRequest.PlaceId);

                await _context.Favourites.AddAsync(favourite);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm yêu thích cho ID {UserId}", userId);
                return false;
            } 
        }

        public async Task<bool> RemoveFavouriteAsync(string userId, FavouriteRequest favouriteRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
                }
                if (favouriteRequest == null)
                {
                    throw new ArgumentNullException(nameof(favouriteRequest), "Favourite request cannot be null.");
                }
                var favourite = await _context.Favourites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.PlaceId == favouriteRequest.PlaceId);
                if (favourite == null)
                {
                    return false;
                }
                _context.Favourites.Remove(favourite);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa yêu thích cho ID {UserId}", userId);
                return false;
            }
        }

        public async Task<IEnumerable<FavouriteResponse>> GetFavouritesByUserIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
                }

                var favourites = await _context.Favourites
                    .Where(f => f.UserId == userId)
                    .Join(
                        _context.Places,
                        favourite => favourite.PlaceId,
                        place => place.Id,
                        (favourite, place) => new { favourite, place }
                    )
                    .GroupJoin(
                        _context.PlaceImages,
                        fp => fp.place.Id,
                        image => image.PlaceId,
                        (fp, images) => new { fp.favourite, fp.place, Image = images.FirstOrDefault() }
                    )
                    .Select(result => new FavouriteResponse
                    {
                        Id = result.place.Id,
                        Name = result.place.Name,
                        Images = result.Image != null ? result.Image.ImageUrl : string.Empty,
                        Rating = result.place.Rating,
                        Address = result.place.Address,
                        Price = result.place.Price,
                        NumOfRating = result.place.NumOfRating
                    })
                    .ToListAsync();

                return favourites;
            }
            catch (ArgumentException argEx)
            {
                throw argEx;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách yêu thích.", ex);
            }
        }

    }
}
