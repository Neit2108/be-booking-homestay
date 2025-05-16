using System.Security.Claims;
using HomestayBookingAPI.DTOs.Favourite;
using HomestayBookingAPI.Services.FavouriteServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomestayBookingAPI.Controllers
{
    [Route("favourite")]
    [ApiController]
    public class FavouriteController : ControllerBase
    {
        private readonly IFavouriteService _favouriteService;
        public FavouriteController(IFavouriteService favouriteService)
        {
            _favouriteService = favouriteService;
        }
        [HttpPost("add")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddFavourite([FromBody] FavouriteRequest favouriteRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }
            var result = await _favouriteService.AddFavouriteAsync(userId, favouriteRequest);
            if (result)
            {
                return Ok("Favourite added successfully.");
            }
            return BadRequest("Failed to add favourite.");
        }

        [HttpDelete("remove")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> RemoveFavourite([FromBody] FavouriteRequest favouriteRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }
            var result = await _favouriteService.RemoveFavouriteAsync(userId, favouriteRequest);
            if (result)
            {
                return Ok("Favourite removed successfully.");
            }
            return BadRequest("Failed to remove favourite.");
        }

        [HttpGet("get-by-user-id")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetFavouritesByUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }
            var favourites = await _favouriteService.GetFavouritesByUserIdAsync(userId);
            return Ok(favourites);
        }
    }
}
