using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Booking;
using HomestayBookingAPI.Models.Enum;
using HomestayBookingAPI.Services.BookingServices;
using HomestayBookingAPI.Services.PlaceServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HomestayBookingAPI.Controllers
{
    [Route("bookings")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IPlaceService _placeService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingService bookingService, ApplicationDbContext context, IPlaceService placeService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _context = context;
            _placeService = placeService;
            _logger = logger;
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

        [HttpPut("accept-booking-request/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
        public async Task<IActionResult> AcceptBookingRequest(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound($"No booking found with ID {id}.");
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentRole = User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(currentRole))
                {
                    _logger.LogWarning("Invalid user claims for accepting booking {BookingId}", id);
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                if (currentRole != "Admin")
                {
                    var place = await _context.Places.FirstOrDefaultAsync(p => p.Id == booking.PlaceId);
                    if (place == null)
                    {
                        _logger.LogWarning("Place with ID {PlaceId} not found for booking {BookingId}", booking.PlaceId, id);
                        return NotFound(new { message = $"Place with ID {booking.PlaceId} not found." });
                    }

                    if (place.OwnerId != currentUserId)
                    {
                        _logger.LogWarning("User {UserId} is not authorized to accept booking {BookingId}", currentUserId, id);
                        return Forbid("You are not authorized to accept this booking");
                    }
                }
                var result = await _bookingService.UpdateBookingStatusAsync(id, BookingStatus.Confirmed, currentRole);
                if (!result)
                {
                    return BadRequest(new { message = "Không thể chấp nhận" });
                }
                return Ok(new {
                    message = "Đã chấp nhận đơn đặt",
                    bookingId = id,
                    status = BookingStatus.Confirmed
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error accepting booking: {ex.Message}");
            }
        }

        [HttpPut("reject-booking-request/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
        public async Task<IActionResult> RejectBookingRequest(int id, [FromBody] RejectBookingRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Reason))
                {
                    _logger.LogWarning("Reject reason is empty for booking {BookingId}", id);
                    return BadRequest(new { message = "Lý do từ chối không được để trống" });
                }

                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound($"No booking found with ID {id}.");
                }
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentRole = User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(currentRole))
                {
                    _logger.LogWarning("Invalid user claims for accepting booking {BookingId}", id);
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                if (currentRole != "Admin")
                {
                    var place = await _context.Places.FirstOrDefaultAsync(p => p.Id == booking.PlaceId);
                    if (place == null)
                    {
                        _logger.LogWarning("Place with ID {PlaceId} not found for booking {BookingId}", booking.PlaceId, id);
                        return NotFound(new { message = $"Place with ID {booking.PlaceId} not found." });
                    }

                    if (place.OwnerId != currentUserId)
                    {
                        _logger.LogWarning("User {UserId} is not authorized to accept booking {BookingId}", currentUserId, id);
                        return Forbid("You are not authorized to accept this booking");
                    }
                }
                var result = await _bookingService.UpdateBookingStatusAsync(id, BookingStatus.Cancelled, currentRole , request.Reason);
                if (!result)
                {
                    _logger.LogWarning("Failed to reject booking {BookingId} with reason: {RejectReason}", id, request.Reason);
                    return BadRequest(new { message = "Không thể từ chối" });
                }
                return Ok(new { 
                    message = "Đã từ chối đơn đặt",
                    rejectReason = request.Reason,
                    bookingId = id,
                    status = BookingStatus.Cancelled
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error rejecting booking: {ex.Message}");
            }
        }

        [HttpGet("can-comment/{placeId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CanComment(int placeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid user authentication" });
            }
            var hasConfirmedBooking = await _bookingService.HasConfirmedBookingAsync(userId, placeId);
            if (hasConfirmedBooking)
            {
                return Ok(new { canComment = true });
            }
            else
            {
                return Ok(new { canComment = false, message = "Bạn không thể bình luận vì chưa có đơn đặt nào" });
            }
        }
    }
}
