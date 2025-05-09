using HomestayBookingAPI.DTOs.Wallet;
using HomestayBookingAPI.Services.WalletServices;
using HomestayBookingAPI.Services.PaymentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HomestayBookingAPI.DTOs.Payment;
using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.Controllers
{
    [Route("wallet")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IVNPayService _vnpayService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(IWalletService walletService, IVNPayService vnpayService, ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _vnpayService = vnpayService;
            _logger = logger;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var balance = await _walletService.GetBalanceAsync(userId);

                return Ok(new { balance = balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số dư ví");
                return StatusCode(500, new { message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] WalletDepositRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Tạo yêu cầu thanh toán
                var paymentRequest = new GenericPaymentRequest
                {
                    Amount = request.Amount,
                    ReturnUrl = request.ReturnUrl,
                    Purpose = PaymentPurpose.WalletDeposit,
                    OrderInfo = $"Nạp {request.Amount:N0} VND vào ví",
                    BankCode = request.BankCode
                };

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "::1";
                var response = await _vnpayService.CreateGenericPaymentAsync(paymentRequest, userId, ipAddress);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo yêu cầu nạp tiền");
                return StatusCode(500, new { message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var transactions = await _walletService.GetTransactionHistoryAsync(userId, page, pageSize);

                var response = transactions.Select(t => new WalletTransactionResponse
                {
                    Id = t.Id,
                    WalletId = t.WalletId,
                    Amount = t.Amount,
                    Type = t.Type.ToString(),
                    Description = t.Description,
                    BookingId = t.BookingId,
                    PaymentId = t.PaymentId,
                    CreatedAt = t.CreatedAt
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử giao dịch");
                return StatusCode(500, new { message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        [HttpPost("set-pin")]
        public async Task<IActionResult> SetPin([FromBody] SetPinRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var result = await _walletService.SetPinAsync(userId, request.Pin);

                if (result)
                {
                    return Ok(new { message = "Đã thiết lập mã PIN thành công" });
                }
                else
                {
                    return BadRequest(new { message = "Không thể thiết lập mã PIN" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thiết lập mã PIN");
                return StatusCode(500, new { message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        [HttpGet("has-pin")]
        public async Task<IActionResult> HasPin()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var hasPin = await _walletService.HasSetPinAsync(userId);

                return Ok(new { hasPin = hasPin });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra PIN");
                return StatusCode(500, new { message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }


    }
}