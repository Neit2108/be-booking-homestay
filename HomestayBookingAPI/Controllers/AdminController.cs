using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomestayBookingAPI.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        [HttpGet("login")]
        public IActionResult Login() => View(); // trả về form login

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Giả sử có admin hardcoded hoặc lấy từ DB
            if (username == "Admin@homies.com" && password == "Admin@123")
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin")
            };

                var claimsIdentity = new ClaimsIdentity(claims, "HangfireCookie");

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync("HangfireCookie", new ClaimsPrincipal(claimsIdentity), authProperties);

                return Redirect("/hangfire");
            }

            return Unauthorized();
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("HangfireCookie");
            return Redirect("/admin/login");
        }
    }
}
