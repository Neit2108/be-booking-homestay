
using HomestayBookingAPI.Data;
using HomestayBookingAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace HomestayBookingAPI.Services.TopRatePlaceServices
{
    public class TopRateService : ITopRateService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TopRatePlaces> _logger;

        public TopRateService(ApplicationDbContext context, ILogger<TopRatePlaces> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpdateTopRateAsync(int limit)
        {
            try
            {
                _logger.LogInformation("Update lúc : " + DateTime.UtcNow);

                var topPlaces = await _context.Places
                        .Include(p => p.Images)
                        .OrderByDescending(p => p.Rating)
                        .Take(limit)
                        .ToListAsync();

                await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"TopRatePlaces\"");

                var newTopPlaces = topPlaces.Select((p, i) => new TopRatePlaces
                {
                    PlaceId = p.Id,
                    Rating = p.Rating,
                    Rank = i + 1,
                    LastUpdated = DateTime.UtcNow
                }).ToList();

                await _context.TopRatePlaces.AddRangeAsync(newTopPlaces);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("Lỗi cập nhật dữ liệu đồng thời.");
                throw new Exception("Lỗi cập nhật dữ liệu đồng thời.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật dữ liệu.");
                throw new Exception("Lỗi khi cập nhật dữ liệu.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định.");
                throw new Exception("Lỗi không xác định.", ex);
            }
        }
    }
}
