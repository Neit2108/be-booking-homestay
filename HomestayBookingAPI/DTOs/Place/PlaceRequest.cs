namespace HomestayBookingAPI.DTOs.Place
{
    public class PlaceRequest
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int MaxGuests { get; set; }
        public string OwnerId { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}
