using HomestayBookingAPI.Services.SalesStatisticsServices;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HomestayBookingAPI.Controllers
{
    [Route("statistics")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class StatisticsController : ControllerBase
    {
        private readonly ISalesStatisticsService _salesStatisticsService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(
            ISalesStatisticsService salesStatisticsService,
            ILogger<StatisticsController> logger)
        {
            _salesStatisticsService = salesStatisticsService;
            _logger = logger;
        }

        [HttpGet("sales")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Landlord")]
        public async Task<IActionResult> GetLandlordSales(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var salesStats = await _salesStatisticsService.GetLandlordSalesStatisticsAsync(userId, startDate, endDate);
                return Ok(salesStats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê doanh số");
                return StatusCode(500, new { message = $"Lỗi khi lấy thống kê doanh số: {ex.Message}" });
            }
        }

        [HttpGet("admin/sales")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> GetAdminSales(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var salesStats = await _salesStatisticsService.GetAdminSalesStatisticsAsync(startDate, endDate);
                return Ok(salesStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê doanh số hệ thống");
                return StatusCode(500, new { message = $"Lỗi khi lấy thống kê doanh số: {ex.Message}" });
            }
        }
    }
}