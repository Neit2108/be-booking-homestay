using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Services.ProfileServices;
using HomestayBookingAPI.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HomestayBookingAPI.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly IProfileService _profileService;
        private readonly ApplicationDbContext _context;

        public UserController(IUserService userService, ILogger<UserController> logger, IProfileService profileService, ApplicationDbContext context)
        {
            _userService = userService;
            _logger = logger;
            _profileService = profileService;
            _context = context;
        }

        [HttpGet("profile")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                if (!HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader) ||
                    !authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid or missing Authorization header");
                    return BadRequest(new { message = "Authorization header không hợp lệ" });
                }

                var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng trong token" });
                }

                var user = await _userService.GetUserByID(id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                return Ok(new
                {
                    message = "Lấy thông tin người dùng thành công",
                    data = new
                    {
                        id = id,
                        name = user.FullName,
                        email = user.Email,
                        phone = user.PhoneNumber,
                        add = user.HomeAddress,
                        birthday = user.BirthDate,
                        gender = user.Gender,
                        avatar = user.AvatarUrl,
                        bio = user.Bio,
                        identityCard = user.IdentityCard,
                        role = _context.UserRoles
                            .Where(ur => ur.UserId == id)
                            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                            .ToList(),
                        createAt = user.CreateAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi truy xuất thông tin người dùng" });
            }
        }

        [HttpPut("update-profile")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileDTO model)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("User ID not found in token");
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng trong token" });
            }

            var success = await _profileService.UpdateProfileAsync(id, model);

            if (!success)
            {
                _logger.LogWarning("Failed to update profile for user with ID {UserId}", id);
                
                return BadRequest();
            }
            
            var updatedUser = await _userService.GetUserByID(id);

            return Ok(new
            {
                message = "Update thành công",
                data = new
                {
                    
                    name = updatedUser.FullName,
                    email = updatedUser.Email,
                    phone = updatedUser.PhoneNumber,
                    add = updatedUser.HomeAddress,
                    birthday = updatedUser.BirthDate,
                    gender = updatedUser.Gender,
                    avatar = updatedUser.AvatarUrl,
                    bio = updatedUser.Bio,
                    identityCard = updatedUser.IdentityCard
                }

            });
        }

        [HttpGet("bulk")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
        public async Task<IActionResult> GetUserBulk([FromQuery] string ids)
        {
            if(string.IsNullOrEmpty(ids))
            {
                return BadRequest(new { message = "Không có id người dùng nào được cung cấp" });
            }
            var userIds = ids.Split(',').Select(id => id.Trim()).ToList();
            if(!userIds.Any())
            {
                return BadRequest(new { message = "Không có id người dùng nào được cung cấp" });
            }
            try
            {
                var users = await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new
                    {
                        id = u.Id,
                        name = u.FullName ?? "Unknown User",
                        email = u.Email
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi truy xuất thông tin người dùng" });
            }

        }

    }
}
