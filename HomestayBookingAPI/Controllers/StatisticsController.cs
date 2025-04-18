using HomestayBookingAPI.Services.StatisticsServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomestayBookingAPI.Controllers
{
    [Route("statistics")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            try
            {
                var statistics = await _statisticsService.GetStatisticsAsync(role, id);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy thống kê: {ex.Message}");
            }
        }
    }
}
