﻿using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Services.ProfileServices;
using HomestayBookingAPI.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public UserController(IUserService userService, ILogger<UserController> logger, IProfileService profileService)
        {
            _userService = userService;
            _logger = logger;
            _profileService = profileService;
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
                        name = user.FullName,
                        email = user.Email,
                        phone = user.PhoneNumber,
                        add = user.HomeAddress,
                        birthday = user.BirthDate,
                        gender = user.Gender,
                        avatar = user.AvatarUrl,
                        bio = user.Bio
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
                    bio = updatedUser.Bio
                }

            });
        }

    }
}
