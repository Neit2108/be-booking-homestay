using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Booking;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using HomestayBookingAPI.Services.PlaceServices;
using HomestayBookingAPI.Services.UserServices;
using Microsoft.EntityFrameworkCore;

namespace HomestayBookingAPI.Services.BookingServices
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingService> _logger;
        private readonly IPlaceService _placeService;
        private readonly IUserService _userService;

        public BookingService(ApplicationDbContext context, ILogger<BookingService> logger, IPlaceService placeService, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _placeService = placeService;
            _userService = userService;
        }

        public async Task<double> CalculateTotalPriceAsync(int placeId, DateTime startDate, DateTime endDate, int numberOfGuests)
        {
            int numberOfDays = (endDate - startDate).Days;
            if (numberOfDays <= 0)
            {
                throw new ArgumentException("End date must be after start date");
            }
            var place = await _placeService.GetPlaceByID(placeId);
            var pricePerNight = place.Price;
            var totalPrice = pricePerNight * numberOfDays;
            if(numberOfGuests > place.MaxGuests)
            {
                totalPrice += (numberOfGuests - place.MaxGuests) * 1.5 * numberOfDays;
            }
            return totalPrice;

        }

        public async Task<bool> CheckAvailabilityAsync(int placeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // Validation
                if (startDate >= endDate)
                {
                    _logger.LogWarning("Invalid date range: StartDate {StartDate} must be earlier than EndDate {EndDate}", startDate, endDate);
                    throw new ArgumentException("StartDate must be earlier than EndDate");
                }

                // Kiểm tra xem place có tồn tại không
                var placeExists = await _placeService.GetPlaceByID(placeId);
                if (placeExists == null)
                {
                    _logger.LogWarning("Place with ID {PlaceId} does not exist", placeId);
                    throw new Exception("Place does not exist");
                }

                // Chuẩn hóa startDate và endDate (bỏ thời gian, chỉ lấy ngày)
                var start = startDate.Date;
                var end = endDate.Date;

                // Kiểm tra tính khả dụng trong bảng PlaceAvailable
                var unavailableDays = await _context.PlaceAvailables
                    .Where(pa => pa.PlaceId == placeId &&
                                 pa.Date >= start &&
                                 pa.Date <= end &&
                                 !pa.IsAvailable) 
                    .AnyAsync();

                if (unavailableDays)
                {
                    _logger.LogInformation("Place {PlaceId} is not available from {StartDate} to {EndDate}", placeId, startDate, endDate);
                    return false;
                }

                _logger.LogInformation("Place {PlaceId} is available from {StartDate} to {EndDate}", placeId, startDate, endDate);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for place {PlaceId} from {StartDate} to {EndDate}", placeId, startDate, endDate);
                throw new Exception("Error checking availability", ex);
            }
        }

        public async Task<BookingResponse> CreateBookingAsync(BookingRequest bookingRequest)
        {
            if (bookingRequest == null)
            {
                throw new ArgumentNullException(nameof(bookingRequest), "BookingDTO cannot be null");
            }
            if (bookingRequest.StartDate >= bookingRequest.EndDate)
            {
                throw new ArgumentException("StartDate must be earlier than EndDate");
            }

            var isAvailable = await CheckAvailabilityAsync(bookingRequest.PlaceId, bookingRequest.StartDate, bookingRequest.EndDate);

            if (!isAvailable)
            {
                throw new Exception("Place is not available for the selected dates");
            }
            try
            {
                var start = bookingRequest.StartDate.Date;
                var end = bookingRequest.EndDate.Date;

                var placeAvailables = await _context.PlaceAvailables
                    .Where(pa => pa.PlaceId == bookingRequest.PlaceId &&
                                 pa.Date >= start &&
                                 pa.Date <= end)
                    .ToListAsync();

                // Nếu không có bản ghi PlaceAvailable cho các ngày này, tạo mới
                for (var date = start; date <= end; date = date.AddDays(1))
                {
                    var existing = placeAvailables.FirstOrDefault(pa => pa.Date == date);
                    if (existing == null)
                    {
                        _context.PlaceAvailables.Add(new PlaceAvailable
                        {
                            PlaceId = bookingRequest.PlaceId,
                            Date = date,
                            IsAvailable = false,
                            Price = bookingRequest.TotalPrice / (end - start).Days // Giả sử giá chia đều cho các ngày
                        });
                    }
                    else
                    {
                        existing.IsAvailable = false;
                    }
                }
                var booking = new Booking
                {
                    UserId = bookingRequest.UserId,
                    PlaceId = bookingRequest.PlaceId,
                    StartDate = bookingRequest.StartDate,
                    EndDate = bookingRequest.EndDate,
                    NumberOfGuests = bookingRequest.NumberOfGuests,
                    TotalPrice = await CalculateTotalPriceAsync(bookingRequest.PlaceId, bookingRequest.StartDate, bookingRequest.EndDate, bookingRequest.NumberOfGuests),
                    Status = bookingRequest.Status,
                    PaymentStatus = PaymentStatus.Unpaid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();

                return new BookingResponse
                {
                    Id = booking.Id,
                    UserId = booking.UserId,
                    PlaceId = booking.PlaceId,
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate,
                    NumberOfGuests = booking.NumberOfGuests,
                    TotalPrice = booking.TotalPrice,
                    Status = booking.Status
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating booking", ex);
            }
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                {
                    return false;
                }
                //if (booking.Status == BookingStatus.Confirmed)
                //{
                //    throw new InvalidOperationException("Cannot delete a confirmed booking");
                //}
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting booking", ex);
            }
        }

        public async Task<BookingResponse> GetBookingByIdAsync(int id)
        {
            var booking = await _context.Bookings
                .Where(b => b.Id == id)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    PlaceId = b.PlaceId,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    NumberOfGuests = b.NumberOfGuests,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status
                })
                .FirstOrDefaultAsync();
            if (booking == null)
            {
                _logger.LogWarning($"Booking with ID {id} not found");
                return null;
            }
            return booking;
        }

        public async Task<IEnumerable<BookingResponse>> GetBookingsByPlaceIdAsync(int placeId)
        {
            var place = await _placeService.GetPlaceByID(placeId);
            
            var bookings = await _context.Bookings
                .Where(b => b.PlaceId == placeId)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    PlaceId = b.PlaceId,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    NumberOfGuests = b.NumberOfGuests,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status
                })
                .ToListAsync();

            if (!bookings.Any())
            {
                _logger.LogInformation("No bookings found for place {PlaceId}", placeId);
            }

            return bookings;
        }

        public async Task<IEnumerable<BookingResponse>> GetBookingsByUserIdAsync(string userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    PlaceId = b.PlaceId,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    NumberOfGuests = b.NumberOfGuests,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status
                })
                .ToListAsync();

            if (!bookings.Any())
            {
                _logger.LogInformation("No bookings found for user {UserId}", userId);
            }

            return bookings;
        }

        public async Task<bool> UpdateBookingStatusAsync(int id, BookingStatus status)
        {
            try
            {
                // Tìm booking theo ID
                var booking = await _context.Bookings
                    .Include(b => b.Place) // Include Place để lấy thông tin liên quan nếu cần
                    .FirstOrDefaultAsync(b => b.Id == id);
                if (booking == null)
                {
                    _logger.LogWarning("Booking with ID {BookingId} not found for status update", id);
                    return false;
                }

                // Kiểm tra logic trạng thái
                if (!IsValidStatusTransition(booking.Status, status))
                {
                    _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} for booking {BookingId}", booking.Status, status, id);
                    throw new Exception($"Invalid status transition from {booking.Status} to {status}");
                }

                // Nếu chuyển sang trạng thái Cancelled, đánh dấu các ngày trong PlaceAvailable là khả dụng
                if (status == BookingStatus.Cancelled && booking.Status != BookingStatus.Cancelled)
                {
                    var start = booking.StartDate.Date;
                    var end = booking.EndDate.Date;

                    var placeAvailables = await _context.PlaceAvailables
                        .Where(pa => pa.PlaceId == booking.PlaceId &&
                                     pa.Date >= start &&
                                     pa.Date <= end)
                        .ToListAsync();

                    foreach (var pa in placeAvailables)
                    {
                        pa.IsAvailable = true; // Đánh dấu lại là khả dụng
                    }
                }

                // Cập nhật trạng thái booking
                booking.Status = status;
                booking.UpdatedAt = DateTime.UtcNow;

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Booking {BookingId} status updated to {Status}", id, status);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while updating status of booking {BookingId}", id);
                throw new Exception("Database error occurred while updating booking status", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating status of booking {BookingId}", id);
                throw new Exception("Unexpected error occurred while updating booking status", ex);
            }
        }

        private bool IsValidStatusTransition(BookingStatus currentStatus, BookingStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                (BookingStatus.Pending, BookingStatus.Confirmed) => true,
                (BookingStatus.Pending, BookingStatus.Cancelled) => true,
                (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,
                _ => false
            };
        }
    }
}
