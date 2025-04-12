using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Booking;
using HomestayBookingAPI.Services.BookingServices;
using HomestayBookingAPI.Services.PlaceServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomestayBookingAPI.Controllers
{
    [Route("bookings")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IPlaceService _placeService;
        private readonly ApplicationDbContext _context;

        public BookingController(IBookingService bookingService, ApplicationDbContext context, IPlaceService placeService)
        {
            _bookingService = bookingService;
            _context = context;
            _placeService = placeService;
        }

        [HttpGet("all-bookings")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BookingResponse>>> GetAllBookings(
            [FromQuery] string? status = null, 
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync(status, startDate, endDate, page, pageSize);
                if (bookings == null || !bookings.Any())
                {
                    return NotFound(new { error = "no_bookings_found", message = "No bookings found." });
                }
                return Ok(new
                {
                    data = bookings,
                    totalRecords = await _context.Bookings.CountAsync()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "internal_server_error", message = "An error occurred while fetching bookings." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponse>> GetBookingById(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound($"No booking found with ID {id}.");
            }
            return Ok(booking);
        }

        [HttpPost("new-booking")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequest bookingDTO)
        {
            if (bookingDTO == null)
            {
                return BadRequest("Booking data is required.");
            }
            try
            {
                var createdBooking = await _bookingService.CreateBookingAsync(bookingDTO);
                return CreatedAtAction(nameof(GetBookingById), new { id = createdBooking.Id }, createdBooking);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating booking: {ex.Message}");
            }
        }

        [HttpGet("user-bookings/{userId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<IEnumerable<BookingResponse>>> GetBookingsByUserId(string userId)
        {
            var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
            if (bookings == null || !bookings.Any())
            {
                return NotFound($"No bookings found for user ID {userId}.");
            }
            return Ok(bookings);
        }

        [HttpGet("landlord-s-bookings/{landlordId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
        public async Task<ActionResult<IEnumerable<BookingResponse>>> GetAllBookingOfLandlord(string landlordId)
        {
            var places = await _placeService.GetAllPlacesOfLandlord(landlordId);

            var bookings = new List<BookingResponse>();
            foreach (var place in places)
            {
                var placeBookings = await _bookingService.GetBookingsByPlaceIdAsync(place.Id);
                if (placeBookings != null && placeBookings.Any())
                {
                    bookings.AddRange(placeBookings);
                }
            }
            if (bookings == null || !bookings.Any())
            {
                return NotFound($"No bookings found for landlord ID {landlordId}.");
            }
            return Ok(bookings);
        }
    }
}
