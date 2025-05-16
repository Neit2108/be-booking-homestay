using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.Place;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using HomestayBookingAPI.Services.PlaceServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HomestayBookingAPI.Controllers
{
    [Route("places")]
    [ApiController]
    
    public class PlaceController : ControllerBase
    {
        private readonly IPlaceService _placeService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlaceController> _logger;

        public PlaceController(IPlaceService placeService, ApplicationDbContext context, ILogger<PlaceController> logger)
        {
            _placeService = placeService;
            _context = context;
            _logger = logger;
        }

        [HttpGet("top-rating")]
        public async Task<ActionResult<List<PlaceDTO>>> GetTopRatingPlaces(int limit = 5)
        {
            string userId = null;

            // Kiểm tra header Authorization
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                try
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                    if (string.IsNullOrEmpty(userId))
                    {
                        _logger.LogDebug("UserId not found in token");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing token, proceeding without userId");
                }
            }
            else
            {
                _logger.LogDebug("No valid Bearer token provided");
            }
            if (limit <= 0)
            {
                return BadRequest("Số lượng phải lớn hơn 0");
            }

            var places = await _placeService.GetTopRatePlace(limit, userId);
            return Ok(places);
        }

        [HttpGet("place-details/{id}")]
        public async Task<ActionResult<PlaceDTO>> GetPlaceById(int id)
        {
            string userId = null;

            // Kiểm tra header Authorization
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                try
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                    if (string.IsNullOrEmpty(userId))
                    {
                        _logger.LogDebug("UserId not found in token");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing token, proceeding without userId");
                }
            }
            else
            {
                _logger.LogDebug("No valid Bearer token provided");
            }
            var place = await _placeService.GetPlaceByID(id, userId);
            if (place == null)
            {
                return NotFound($"Không tìm thấy Place với Id {id}.");
            }
            return Ok(place);
        }


        [HttpGet("get-all")]
        public async Task<ActionResult<List<PlaceDTO>>> GetAllPlaces()
        {
            string userId = null;

            // Kiểm tra header Authorization
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                try
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                    if (string.IsNullOrEmpty(userId))
                    {
                        _logger.LogDebug("UserId not found in token");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing token, proceeding without userId");
                }
            }
            else
            {
                _logger.LogDebug("No valid Bearer token provided");
            }

            // Lấy danh sách địa điểm
            var places = await _placeService.GetAllPlacesAsync(userId);
            return Ok(places);
        }


        [HttpGet("get-all-for-landlord/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
        public async Task<ActionResult<List<PlaceDTO>>> GetAllPlacesForLandlord(string id)
        {

            var places = await _placeService.GetAllPlacesOfLandlord(id);
            return Ok(places);
        }

        [HttpGet("get-same-category/{id}")]
        public async Task<ActionResult<List<PlaceDTO>>> GetSameCategoryPlaces(int id)
        {
            string userId = null;

            // Kiểm tra header Authorization
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                try
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                    if (string.IsNullOrEmpty(userId))
                    {
                        _logger.LogDebug("UserId not found in token");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing token, proceeding without userId");
                }
            }
            else
            {
                _logger.LogDebug("No valid Bearer token provided");
            }
            var sameCategoryPlaces = await _placeService.GetSameCategoryPlaces(id, userId);
            if (sameCategoryPlaces == null || sameCategoryPlaces.Count == 0)
            {
                return NotFound($"Không tìm thấy danh sách địa điểm cùng loại với Id {id}.");
            }
            return Ok(sameCategoryPlaces);
        }

        [HttpPost("add-place")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
        public async Task<IActionResult> AddPlace([FromForm] PlaceRequest placeRequest)
        {

            try
            {
                var newPlace = await _placeService.AddPlaceAsync(placeRequest);
                return CreatedAtAction(
                    nameof(GetPlaceById), // Tên action để lấy Place theo Id
                    new { id = newPlace.Id }, // Route values
                    newPlace // Dữ liệu trả về
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("upload-image/{placeId}")]
        public async Task<ActionResult<List<string>>> UploadImagePlace(int placeId, [FromForm] List<IFormFile> images)
        {
            if (images == null || images.Count == 0)
            {
                return BadRequest("Không có ảnh nào được tải lên haha");
            }

            const int MAX_IMAGE_COUNT = 10;
            if (images.Count > MAX_IMAGE_COUNT)
            {
                return BadRequest($"Số lượng ảnh tối đa là {MAX_IMAGE_COUNT}");
            }
            try
            {
                var imageUrls = await _placeService.UploadImagePlaceAsync(placeId, images);

                if (imageUrls == null || imageUrls.Count == 0)
                {
                    return BadRequest("Không có ảnh nào được tải lên");
                }

                return Ok(imageUrls);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpGet("bulk")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
        public async Task<IActionResult> GetPlacesBulk([FromQuery] string ids)
        {
            if (string.IsNullOrEmpty(ids.ToString()))
            {
                return BadRequest("Không có id nào được cung cấp á");
            }
            var placeIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(id => int.Parse(id.Trim()))
                          .ToList();
            if (!placeIds.Any())
            {
                return BadRequest("Không có id nào được cung cấp");
            }
            try
            {
                var places = await _context.Places
                    .Where(p => placeIds.Contains(p.Id))
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name ?? "Unknown Place",
                        address = p.Address
                    })
                    .ToListAsync();
                return Ok(places);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách địa điểm: {ex.Message}");
            }
        }

        [HttpPut("update-status")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
        public async Task<IActionResult> UpdatePlaceStatus([FromBody] UpdatePlaceStatusRequest request)
        {
            if (request == null)
            {
                _logger.LogWarning("Invalid request: request is null");
                return BadRequest("Yêu cầu không hợp lệ");
            }

            // Nếu chuyển từ Active sang Inactive thì kiểm tra ngày
            if (request.NewStatus == PlaceStatus.Inactive)
            {
                if (request.InactiveFrom == null && request.InactiveTo == null)
                {
                    _logger.LogWarning("Invalid request: missing dates for Inactive status");
                    return BadRequest("Thiếu ngày tháng khi inactive");
                }

                if (request.InactiveFrom != null && request.InactiveTo != null && request.InactiveFrom > request.InactiveTo)
                {
                    _logger.LogWarning("Invalid request: InactiveFrom > InactiveTo");
                    return BadRequest("Ngày bắt đầu không thể lớn hơn ngày kết thúc");
                }
            }

            var place = await _context.Places.FindAsync(request.PlaceId);
            if (place == null)
            {
                _logger.LogWarning($"Place not found with ID {request.PlaceId}");
                return NotFound($"Không tìm thấy địa điểm với ID {request.PlaceId}");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (currentUserRole != "Admin" && place.OwnerId != currentUserId)
            {
                _logger.LogWarning($"User {currentUserId} not authorized to update status of place {request.PlaceId}");
                return Forbid("Bạn không có quyền cập nhật trạng thái địa điểm này");
            }

            if (place.Status == request.NewStatus)
            {
                return BadRequest("Trạng thái không thay đổi");
            }

            try
            {
                var isUpdated = await _placeService.UpdatePlaceStatusAsync(request);
                if (isUpdated)
                {
                    _logger.LogInformation($"Successfully updated status of place {request.PlaceId} to {request.NewStatus}");
                    return Ok(new
                    {
                        message = "Cập nhật trạng thái thành công",
                        placeId = request.PlaceId,
                        status = request.NewStatus.ToString()
                    });
                }
                else
                {
                    _logger.LogWarning($"Failed to update status of place {request.PlaceId}");
                    return BadRequest("Cập nhật trạng thái thất bại");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status of place {request.PlaceId}");
                return StatusCode(500, $"Lỗi khi cập nhật trạng thái: {ex.Message}");
            }
        }
    }
}
