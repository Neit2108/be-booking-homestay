using HomestayBookingAPI.DTOs.Promotion;
using HomestayBookingAPI.DTOs.Voucher;
using HomestayBookingAPI.Services.PromotionServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomestayBookingAPI.Controllers
{
    [Route("promotions")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;
        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet("all-promotions")]
        public async Task<IActionResult> GetAllPromotions()
        {
            var promotions = await _promotionService.GetAllPromotionsAsync();
            return Ok(promotions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPromotionById(int id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }
            return Ok(promotion);
        }

        [HttpPost("create-promotion")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
        public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
        {
            if (request.Promotion == null || request.Voucher == null)
            {
                return BadRequest("Promotion or Voucher request cannot be null");
            }
            var promotion = await _promotionService.CreatePromotionAsync(request.Promotion, request.Voucher);
            return CreatedAtAction(nameof(GetPromotionById), new { id = promotion.Id }, promotion);
        }
    }
}
