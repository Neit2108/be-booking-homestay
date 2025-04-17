﻿namespace HomestayBookingAPI.DTOs.Place
{
    public class PlaceResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Rating { get; set; }
        public int NumOfRating { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int MaxGuests { get; set; }
        public string OwnerId { get; set; }
        public List<PlaceImageDTO> Images { get; set; }
    }
}
