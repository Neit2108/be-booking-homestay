using System.Security.Claims;
using HomestayBookingAPI.Services.NotifyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomestayBookingAPI.Controllers
{
    [Route("notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly INotifyService _notifyService;

        public NotifyController(INotifyService notifyService)
        {
            _notifyService = notifyService;
        }

        [HttpGet("user")]
        [Authorize(AuthenticationSchemes ="Bearer")]
        public async Task<IActionResult> GetAllNotifyAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notis = await _notifyService.GetAllNotifyByUserId(userId);
            return Ok(notis);
        }
    }
}
