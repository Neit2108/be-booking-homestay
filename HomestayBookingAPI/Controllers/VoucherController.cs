using HomestayBookingAPI.DTOs.Voucher;
using HomestayBookingAPI.Services.VoucherServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomestayBookingAPI.Controllers
{
    [Route("utils/voucher")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        [HttpPost("validate")]
        public async Task<ActionResult<VoucherResponse>> ValidateVoucher([FromBody] VoucherRequest voucherRequest)
        {
            var voucher = await _voucherService.CheckVoucherAvailable(voucherRequest.Code);
            if (voucher == null)
            {
                return BadRequest();
            }
            return Ok(voucher);
        }
    }
}
