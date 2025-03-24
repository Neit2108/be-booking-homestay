
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
            _logger.LogInformation("Update lúc : " +  DateTime.UtcNow);

            var topPlaces = await _context.Places
                    .Include(p => p.Images)
                    .OrderByDescending(p => p.Rating)
                    .Take(limit)
                    .ToListAsync();

            var oldTopPlaces = await _context.TopRatePlaces.ToListAsync();
            _context.TopRatePlaces.RemoveRange(oldTopPlaces);

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
    }
}
