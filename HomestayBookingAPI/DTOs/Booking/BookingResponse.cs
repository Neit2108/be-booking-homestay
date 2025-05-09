﻿using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.DTOs.Booking
{
    public class BookingResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int PlaceId { get; set; }
        public string PlaceName { get; set; }
        public string PlaceAddress { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfGuests { get; set; }
        public double TotalPrice { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public string? RejectReason { get; set; } // nếu booking bị hủy
        public string? ImageUrl { get; set; }
    }
}
