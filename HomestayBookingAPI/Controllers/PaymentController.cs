// Controllers/PaymentController.cs
using HomestayBookingAPI.DTOs.Payment;
using HomestayBookingAPI.Services.PaymentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomestayBookingAPI.Controllers
{
    [Route("vnpay")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IVNPayService _vnpayService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IVNPayService vnpayService, ILogger<PaymentController> logger)
        {
            _vnpayService = vnpayService;
            _logger = logger;
        }

        [HttpPost("create-payment")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CreatePayment([FromBody] VNPayCreateRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "::1";
                var response = await _vnpayService.CreatePaymentUrlAsync(request, ipAddress);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment URL");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            _logger.LogInformation("Received callback with data: {data}", string.Join(", ", HttpContext.Request.Query.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            try
            {
                // Lấy tất cả tham số từ query string
                var vnpayData = HttpContext.Request.Query
                    .ToDictionary(x => x.Key, x => x.Value.ToString());

                if (!vnpayData.Any())
                {
                    return BadRequest(new { message = "No VNPay callback data received" });
                }

                var response = await _vnpayService.ProcessPaymentCallbackAsync(vnpayData);


                return Ok(new
                {
                    paymentId = response.Id,
                    status = response.Status,
                    redirectUrl = $"{vnpayData["VNPay:ReturnUrl"]}?paymentId={response.Id}&status={response.Status}"
                });

                // Redirect to frontend payment result page
                //var redirectUrl = $"{vnpayData["vnp_ReturnUrl"]}?paymentId={response.Id}&status={response.Status}";
                //return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay callback");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("payment/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetPayment(int id)
        {
            try
            {
                var payment = await _vnpayService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(new { message = $"Payment with ID {id} not found" });
                }

                // Xác thực người dùng có quyền xem thông tin thanh toán này
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (userRole != "Admin" && payment.UserId != userId)
                {
                    return Forbid();
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("booking/{bookingId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetPaymentsByBookingId(int bookingId)
        {
            try
            {
                var payments = await _vnpayService.GetPaymentsByBookingIdAsync(bookingId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments by booking ID");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetPaymentsByUserId()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var payments = await _vnpayService.GetPaymentsByUserIdAsync(userId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments by user ID");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}