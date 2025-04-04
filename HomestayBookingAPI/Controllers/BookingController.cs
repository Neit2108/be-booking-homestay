using HomestayBookingAPI.DTOs.Booking;
using HomestayBookingAPI.Services.BookingServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomestayBookingAPI.Controllers
{
    [Route("bookings")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("booking-details/{id}")]
        public async Task<ActionResult<BookingResponse>> GetBookingByIdAsync(int id)
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
                return CreatedAtAction(nameof(GetBookingByIdAsync), new { id = createdBooking.Id }, createdBooking);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating booking: {ex.Message}");
            }
        }
    }
}
