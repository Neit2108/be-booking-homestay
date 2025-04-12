using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.DTOs.Booking
{
    public class BookingResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int PlaceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfGuests { get; set; }
        public double TotalPrice { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public string? ImageUrl { get; set; }
    }
}
