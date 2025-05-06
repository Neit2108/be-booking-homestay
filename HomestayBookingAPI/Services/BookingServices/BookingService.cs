using Hangfire;
using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Booking;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using HomestayBookingAPI.Services.NotifyServices;
using HomestayBookingAPI.Services.PlaceServices;
using HomestayBookingAPI.Services.UserServices;
using HomestayBookingAPI.Services.VoucherServices;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HomestayBookingAPI.Services.BookingServices
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingService> _logger;
        private readonly IPlaceService _placeService;
        private readonly IUserService _userService;
        private readonly IVoucherService _voucherService;
        private readonly INotifyService _notifyService;

        public BookingService(ApplicationDbContext context, ILogger<BookingService> logger, IPlaceService placeService, IUserService userService, IVoucherService voucherService, INotifyService notifyService)
        {
            _context = context;
            _logger = logger;
            _placeService = placeService;
            _userService = userService;
            _voucherService = voucherService;
            _notifyService = notifyService;
        }

        public async Task<double> CalculateTotalPriceAsync(BookingRequest bookingRequest)
        {
            var endDate = bookingRequest.EndDate.Date;
            var startDate = bookingRequest.StartDate.Date;
            int numberOfDays = (endDate - startDate).Days + 1;
            if (startDate == endDate)
            {
                numberOfDays = 1;
            }
                
            if (numberOfDays <= 0)
            {
                throw new ArgumentException("End date must be after start date");
            }
            var place = await _placeService.GetPlaceByID(bookingRequest.PlaceId);
            var pricePerNight = place.Price;
            _logger.LogDebug($"Giá tiền mỗi đêm của địa điểm {bookingRequest.PlaceId} là : {pricePerNight}");
            var totalPrice = pricePerNight * numberOfDays;
            _logger.LogDebug($"Total price: {totalPrice}");
            _logger.LogDebug($"Số ngày {numberOfDays}");
            if(bookingRequest.NumberOfGuests >= 3)
            {
                totalPrice += totalPrice * 0.3;
            }
            if ((!string.IsNullOrEmpty(bookingRequest.Voucher)) && (await _voucherService.CheckVoucherAvailable(bookingRequest.Voucher) != null))
            {
                totalPrice = await _voucherService.ApplyVoucherAsync(bookingRequest.Voucher, totalPrice);
                _logger.LogDebug("Voucher được dùng : " + bookingRequest.Voucher);
            }
            _logger.LogDebug("***************" +
                "                   " +
                "                   " +
                "                    " +
                "*********************\n");
            _logger.LogDebug($"Total price (after): {totalPrice}");
            return totalPrice;

        }

        public async Task<bool> CheckAvailabilityAsync(int placeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    _logger.LogWarning("Invalid date range: StartDate {StartDate} must be earlier than EndDate {EndDate}", startDate, endDate);
                    throw new ArgumentException("StartDate must be earlier than EndDate");
                }

                var placeExists = await _placeService.GetPlaceByID(placeId);
                if (placeExists == null)
                {
                    _logger.LogWarning("Place with ID {PlaceId} does not exist", placeId);
                    throw new Exception("Place does not exist");
                }

                var start = startDate.Date;
                var end = endDate.Date;
   
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
                throw new ArgumentNullException(nameof(bookingRequest), "Booking request cannot be null");
            }

            if (string.IsNullOrEmpty(bookingRequest.UserId) || bookingRequest.PlaceId == null)
            {
                throw new ArgumentException("UserId and PlaceId must be provided");
            }

            if (bookingRequest.NumberOfGuests <= 0)
            {
                throw new ArgumentException("Number of guests must be greater than zero");
            }

            if (bookingRequest.StartDate > bookingRequest.EndDate)
            {
                throw new ArgumentException("Start date must be earlier than end date");
            }

            var isAvailable = await CheckAvailabilityAsync(bookingRequest.PlaceId, bookingRequest.StartDate, bookingRequest.EndDate);
            if (!isAvailable)
            {
                throw new InvalidOperationException($"Place {bookingRequest.PlaceId} is not available for the selected dates");
            } //-> Check phòng trống

            var booking = new Booking
            {
                UserId = bookingRequest.UserId,
                PlaceId = bookingRequest.PlaceId,
                StartDate = bookingRequest.StartDate,
                EndDate = bookingRequest.EndDate,
                NumberOfGuests = bookingRequest.NumberOfGuests,
                TotalPrice = await CalculateTotalPriceAsync(bookingRequest),
                Status = bookingRequest.Status,
                PaymentStatus = PaymentStatus.Unpaid,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };


            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var start = bookingRequest.StartDate.Date;
                var end = bookingRequest.EndDate.Date;

                // Các ngày đặt
                var dateRange = Enumerable.Range(0, (end - start).Days + 1)
                    .Select(d => start.AddDays(d))
                    .ToList();

                var placeAvailables = await _context.PlaceAvailables
                    .Where(pa => pa.PlaceId == bookingRequest.PlaceId && dateRange.Contains(pa.Date))
                    .ToDictionaryAsync(pa => pa.Date, pa => pa);

                var updates = new List<PlaceAvailable>();
                foreach (var date in dateRange)
                {
                    if (placeAvailables.TryGetValue(date, out var existing)) 
                    {
                        existing.IsAvailable = false;
                    }
                    else
                    {
                        updates.Add(new PlaceAvailable
                        {
                            PlaceId = bookingRequest.PlaceId,
                            Date = date,
                            IsAvailable = false
                        });
                    }
                }

                if (updates.Any())
                {
                    _context.PlaceAvailables.AddRange(updates);
                }

                _logger.LogDebug($"Giá tiền : {booking.TotalPrice}");
                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();

                await _notifyService.CreateNewBookingNotificationAsync(booking); //tao thong bao

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to create booking due to an unexpected error", ex);
            }

            return new BookingResponse
            {
                Id = booking.Id,
                UserId = booking.UserId,
                PlaceId = booking.PlaceId,
                PlaceName = booking.Place.Name,
                PlaceAddress = booking.Place.Address,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                NumberOfGuests = booking.NumberOfGuests,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                PaymentStatus = booking.PaymentStatus
            };
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

        public async Task<IEnumerable<BookingResponse>> GetAllBookingsAsync(
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1,
            int pageSize = 10)
        {
            // Bắt đầu truy vấn
            var query = _context.Bookings.AsQueryable();

            // Áp dụng bộ lọc nếu có
            if (!string.IsNullOrEmpty(status))
            {
                // Chuyển status từ string sang enum để lọc
                if (Enum.TryParse<BookingStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(b => b.Status == statusEnum);
                }
                else
                {
                    _logger.LogWarning($"Invalid status value: {status}");
                    // Có thể throw exception hoặc bỏ qua bộ lọc
                }
            }
            if (startDate.HasValue)
            {
                query = query.Where(b => b.StartDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(b => b.EndDate <= endDate.Value);
            }

            // Tính tổng số bản ghi
            int totalRecords = await query.CountAsync();
            _logger.LogInformation($"Found {totalRecords} bookings before pagination.");

            // Áp dụng phân trang
            query = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            // Truy vấn và ánh xạ dữ liệu
            var bookings = await query
                .Include(b => b.Place)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    PlaceId = b.PlaceId,
                    PlaceAddress = b.Place.Address,
                    PlaceName = b.Place.Name,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    NumberOfGuests = b.NumberOfGuests,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus,
                    ImageUrl = b.Place.Images != null && b.Place.Images.Any() ? b.Place.Images.FirstOrDefault().ImageUrl : null
                })
                .ToListAsync();

            _logger.LogInformation($"Returning {bookings.Count} bookings after applying filters and pagination.");
            return bookings;
        }

        public async Task<BookingResponse> GetBookingByIdAsync(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Place)
                .Where(b => b.Id == id)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    PlaceId = b.PlaceId,
                    PlaceName = b.Place.Name,
                    PlaceAddress = b.Place.Address,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    NumberOfGuests = b.NumberOfGuests,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus,
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
                .Include(b => b.Place)
                .Where(b => b.PlaceId == placeId)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    PlaceId = b.PlaceId,
                    PlaceName = b.Place.Name,
                    PlaceAddress = b.Place.Address,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    NumberOfGuests = b.NumberOfGuests,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus,
                    ImageUrl = place.Images != null && place.Images.Any() ? place.Images.FirstOrDefault().ImageUrl : null
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
                        .Include(b => b.Place)
                        .Where(b => b.UserId == userId)
                        .GroupJoin(_context.Places,
                            b => b.PlaceId,
                            p => p.Id,
                            (b, p) => new { Booking = b, Places = p }) 
                        .SelectMany(x => x.Places.DefaultIfEmpty(),
                            (x, p) => new BookingResponse
                            {
                                Id = x.Booking.Id,
                                UserId = x.Booking.UserId,
                                PlaceId = x.Booking.PlaceId,
                                PlaceName = x.Booking.Place.Name,
                                PlaceAddress = x.Booking.Place.Address,
                                StartDate = x.Booking.StartDate,
                                EndDate = x.Booking.EndDate,
                                NumberOfGuests = x.Booking.NumberOfGuests,
                                TotalPrice = x.Booking.TotalPrice,
                                Status = x.Booking.Status,
                                PaymentStatus = x.Booking.PaymentStatus,
                                ImageUrl = p != null && p.Images != null && p.Images.Any()
                                    ? p.Images.FirstOrDefault().ImageUrl
                                    : null 
                            }) 
                        .ToListAsync();

            if (!bookings.Any())
            {
                _logger.LogInformation("No bookings found for user {UserId}", userId);
            }

            return bookings;
        }

        public async Task<bool> HasConfirmedBookingAsync(string userId, int placeId)
        {
            var hasConfirmedBooking = await _context.Bookings
                .AnyAsync(b => b.UserId == userId && b.PlaceId == placeId && b.Status == BookingStatus.Confirmed);
            return hasConfirmedBooking;
        }

        public async Task<bool> UpdateBookingStatusAsync(int id, BookingStatus status, string currentRole, string rejectReason = "Không xác định")
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tìm booking theo ID
                var booking = await _context.Bookings
                    .Include(b => b.Place) // Include Place để lấy thông tin liên quan nếu cần
                    .ThenInclude(b => b.Owner) // Include Owner để lấy thông tin chủ nhà
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                {
                    _logger.LogWarning("Booking with ID {BookingId} not found for status update", id);
                    return false;
                }
                
                if (!IsValidStatusTransition(booking.Status, status, currentRole))
                {
                    _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} for booking {BookingId}", booking.Status, status, id);
                    throw new Exception($"Invalid status transition from {booking.Status} to {status}");
                }

                var oldStatus = booking.Status;
                booking.Status = status;
                booking.UpdatedAt = DateTime.UtcNow;


                if (status == BookingStatus.Cancelled && booking.Status != BookingStatus.Cancelled)
                {
                    var start = booking.StartDate.Date;
                    var end = booking.EndDate.Date;

                    var placeAvailables = await _context.PlaceAvailables
                        .Where(pa => pa.PlaceId == booking.PlaceId &&
                                     pa.Date >= start &&
                                     pa.Date <= end)
                        .ToListAsync();

                    if (!placeAvailables.Any())
                    {
                        _logger.LogWarning("No PlaceAvailables found for booking {BookingId} in date range {StartDate} to {EndDate}", id, start, end);
                    }
                    else
                    {
                        foreach (var pa in placeAvailables)
                        {
                            if (!pa.IsAvailable) 
                            {
                                pa.IsAvailable = true;
                            }
                        }
                    }
                }

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                if (status == BookingStatus.Confirmed || status == BookingStatus.Cancelled)
                {
                    await _notifyService.NotifyBookingStatusChangeAsync(id, status == BookingStatus.Confirmed, rejectReason);
                }

                await transaction.CommitAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database error occurred while updating status of booking {BookingId}", id);
                throw new Exception("Database error occurred while updating booking status", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error occurred while updating status of booking {BookingId}", id);
                throw new Exception("Unexpected error occurred while updating booking status", ex);
            }
        }

        private bool IsValidStatusTransition(BookingStatus currentStatus, BookingStatus newStatus, string role)
        {
            return (currentStatus, newStatus) switch
            {
                (BookingStatus.Pending, BookingStatus.Confirmed) => true,
                (BookingStatus.Pending, BookingStatus.Cancelled) => true,
                (BookingStatus.Confirmed, BookingStatus.Cancelled) => role == "Admin",
                _ => false
            };
        }
    }
}
