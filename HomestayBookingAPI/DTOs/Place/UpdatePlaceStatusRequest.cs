using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.DTOs.Place
{
    public class UpdatePlaceStatusRequest
    {
        public int PlaceId { get; set; }
        public PlaceStatus NewStatus { get; set; }
        public DateTime? InactiveFrom { get; set; }
        public DateTime? InactiveTo { get; set; } // null có nghĩa là vĩnh viễn
        public string Reason { get; set; }
    }
}
