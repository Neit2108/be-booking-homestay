using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services;
using HomestayBookingAPI.Services.AuthService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace HomestayBookingAPI.Controllers
{
    [Route("account/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            var user = await _authService.RegisterUser(model);
            if (user == null)
            {
                return BadRequest(new { message = "Đăng ký không thành công" });
            }
            return Ok(new { message = "Đăng ký thành công"});
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
            {
            var loginReponse = await _authService.LoginUser(model);
            if (loginReponse == null)
            {
                return BadRequest(new { message = "Đăng nhập không thành công" });
        }
            return Ok(new
            {
                token = loginReponse.Token,
                fullName = loginReponse.FullName,
                avatarUrl = loginReponse.AvatarUrl
            });
            }

        //[HttpPost("logout")]
        //public async Task<IActionResult> Logout()
        //{

        //}

    }
}
