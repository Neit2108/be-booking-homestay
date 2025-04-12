using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.DTOs.Booking
{
    public class BookingRequest
    {
        public string UserId { get; set; }
        public int PlaceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfGuests { get; set; }
        public double TotalPrice { get; set; }
        public string? Voucher { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }
}
