using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Services.OwnerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomestayBookingAPI.Controllers
{
    [Route("owners")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
        private readonly IOwnerService _ownerService;
        public OwnerController(IOwnerService ownerService)
        {
            _ownerService = ownerService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterOwner([FromForm] RegisterOwnerRequest ownerForm, [FromForm] RegisterPlaceRequest placeForm)
        {
            try
            {
                var result = await _ownerService.RegisterOwner(ownerForm, placeForm);
                return Ok(new {message = "Đăng ký chủ nhà thành công"});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
