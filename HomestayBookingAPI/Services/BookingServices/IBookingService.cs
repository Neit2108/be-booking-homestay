using HomestayBookingAPI.DTOs.Booking;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.Services.BookingServices
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(BookingRequest bookingDTO);
        Task<BookingResponse> GetBookingByIdAsync(int id);
        Task<IEnumerable<BookingResponse>> GetBookingsByUserIdAsync(string userId);
        Task<IEnumerable<BookingResponse>> GetBookingsByPlaceIdAsync(int placeId);
        Task<bool> UpdateBookingStatusAsync(int id, BookingStatus status);
        Task<bool> DeleteBookingAsync(int id);
        Task<bool> CheckAvailabilityAsync(int placeId, DateTime startDate, DateTime endDate);
        Task<double> CalculateTotalPriceAsync(int placeId, DateTime startDate, DateTime endDate, int numberOfGuests);
    }
}
