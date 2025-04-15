using HomestayBookingAPI.DTOs.Booking;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.Services.BookingServices
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(BookingRequest bookingRequest);
        Task<BookingResponse> GetBookingByIdAsync(int id);
        Task<IEnumerable<BookingResponse>> GetAllBookingsAsync(string? status, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        Task<IEnumerable<BookingResponse>> GetBookingsByUserIdAsync(string userId);
        Task<IEnumerable<BookingResponse>> GetBookingsByPlaceIdAsync(int placeId);
        Task<bool> UpdateBookingStatusAsync(int id, BookingStatus status, string currentRole, string rejectReason = "Không xác định");
        Task<bool> DeleteBookingAsync(int id);
        Task<bool> CheckAvailabilityAsync(int placeId, DateTime startDate, DateTime endDate);
        Task<double> CalculateTotalPriceAsync(BookingRequest bookingRequest);
    }
}
