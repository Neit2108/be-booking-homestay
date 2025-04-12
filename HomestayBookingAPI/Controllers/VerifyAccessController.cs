using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Services.JwtServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HomestayBookingAPI.Controllers
{
    [Route("account/auth")]
    [ApiController]
    public class VerifyAccessController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;

        public VerifyAccessController(IConfiguration configuration, IJwtService jwtService)
        {
            _configuration = configuration;
            _jwtService = jwtService;
        }

        [HttpPost("verify-action")]
        [AllowAnonymous]
        public IActionResult ValidateActionToken([FromBody] TokenDTO dto)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]);

            try
            {
                var principal = tokenHandler.ValidateToken(dto.Token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"],

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var tokenUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tokenAction = principal.FindFirst("act")?.Value;
                var tokenRefId = principal.FindFirst("ref")?.Value;

                return Ok(new
                {
                    action = tokenAction,
                    referenceId = tokenRefId
                });
            }
            catch (Exception)
            {
                return Unauthorized("Token không hợp lệ hoặc hết hạn");
            }
        }

    }
}
