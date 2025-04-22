using HomestayBookingAPI.DTOs.User;
using HomestayBookingAPI.Services.UserServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomestayBookingAPI.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;

        [HttpGet("all-user")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUser()
        {
            var users = await _userService.GetAllUsersAsync();
            if (users == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }
            return Ok(users);

        }
    }
}
