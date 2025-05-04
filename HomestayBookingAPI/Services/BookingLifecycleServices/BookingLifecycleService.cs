using HomestayBookingAPI.Data;
using HomestayBookingAPI.Models.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomestayBookingAPI.Services.BookingLifecycleServices
{
    public class BookingLifecycleService : IBookingLifecycleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingLifecycleService> _logger;

        public BookingLifecycleService(ApplicationDbContext context, ILogger<BookingLifecycleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessCompletedBookingsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                // Find all confirmed and paid bookings where end date has passed
                var bookingsToComplete = await _context.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed &&
                           b.PaymentStatus == PaymentStatus.Paid &&
                           b.EndDate.Date < today)
                    .ToListAsync();

                if (bookingsToComplete.Any())
                {
                    _logger.LogInformation($"Found {bookingsToComplete.Count} bookings to mark as completed");

                    foreach (var booking in bookingsToComplete)
                    {
                        booking.Status = BookingStatus.Completed;
                        booking.UpdatedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully updated {bookingsToComplete.Count} bookings to Completed status");
                }
                else
                {
                    _logger.LogInformation("No bookings to mark as completed found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing completed bookings");
                throw;
            }
        }

        public async Task CleanupOldBookingsAsync()
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Date.AddDays(-30);

                // Find completed bookings older than 30 days
                var bookingsToDelete = await _context.Bookings
                    .Where(b => b.Status == BookingStatus.Completed &&
                           b.UpdatedAt.Date <= cutoffDate)
                    .ToListAsync();

                if (bookingsToDelete.Any())
                {
                    _logger.LogInformation($"Found {bookingsToDelete.Count} old bookings to delete");

                    _context.Bookings.RemoveRange(bookingsToDelete);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Successfully deleted {bookingsToDelete.Count} old bookings");
                }
                else
                {
                    _logger.LogInformation("No old bookings to delete found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old bookings");
                throw;
            }
        }
    }
}