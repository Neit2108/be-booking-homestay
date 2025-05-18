using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.Password;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services;
using HomestayBookingAPI.Services.AuthService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace HomestayBookingAPI.Controllers
{
    [Route("account/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(IAuthService authService,
            UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            var user = await _authService.RegisterUser(model);
            if (user == null)
            {
                return BadRequest(new { message = "Đăng ký không thành công" });
            }
            return Ok(new { message = "Đăng ký thành công" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var loginReponse = await _authService.LoginUser(model);
            if (loginReponse == null)
            {
                return BadRequest(new { message = "Đăng nhập không thành công" });
            }

            if (loginReponse.RequiresTwoFactor)
            {
                return Ok(new
                {
                    requiresTwoFactor = true,
                    userId = loginReponse.UserId,
                    message = "Tài khoản đã bật xác thực 2 lớp. Vui lòng kiểm tra email để lấy mã OTP."
                });
            }

            return Ok(new
            {
                token = loginReponse.Token,
                fullName = loginReponse.FullName,
                avatarUrl = loginReponse.AvatarUrl,
            });
        }

        [HttpPost("login-2fa")]
        [AllowAnonymous]
        public async Task<IActionResult> Login2FA([FromBody] Login2FADTO model)
        {
            var result = await _authService.LoginUser2FA(model.UserId, model.Otp);

            if (result == null || string.IsNullOrEmpty(result.Token))
                return Unauthorized(new { message = "Mã xác thực không đúng hoặc đã hết hạn" });

            return Ok(new
            {
                token = result.Token,
                fullName = result.FullName,
                avatarUrl = result.AvatarUrl,
            });
        }

        [HttpPost("change-password")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _authService.ChangePasswordAsync(userId, request);
            if (result)
            {
                return Ok(new
                {
                    message = "Đổi mật khẩu thành công"
                });
            }
            return BadRequest(new { message = "Đổi mật khẩu không thành công, vui lòng kiểm tra lại thông tin" });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Email không được để trống" });
            }

            var res = await _authService.ForgotPasswordAsync(request.Email);

            if (res)
            {
                return Ok(new { message = "Mật khẩu mới đã được gửi đến email của bạn" });
            }
            else
            {
                return BadRequest(new { message = "Không thành công" });
            }

        }

        [HttpPost("send-2fa-otp")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> SendTwoFactorOtp()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User không tồn tại");

            var sent = await _authService.SendTwoFactorEnableOtpAsync(user);
            if (!sent) return BadRequest("Không gửi được mã xác thực");

            return Ok(new { message = "Đã gửi mã xác thực đến email" });
        }

        [HttpPost("enable-2fa")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> EnableTwoFactor([FromForm]string token)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User không tồn tại");

            var result = await _authService.EnableTwoFactorAsync(user, token);

            if (!result)
                return BadRequest("Không gửi được mã xác thực");

            return Ok(new { message = "Bật bảo mật 2 lớp thành công" });
        }

        [HttpPost("disable-2fa")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> DisableTwoFactor()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if(user == null)
            {
                return NotFound("User 0 tồn tại");
            }

            var res = await _authService.DisableTwoFactorAsync(user);

            if (!res)
            {
                return BadRequest("Không tắt được");
            }

            return Ok(new { message = "Tắt bảo mật thành công" });
        }
    }
}
