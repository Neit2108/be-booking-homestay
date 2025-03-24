using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.PlaceServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HomestayBookingAPI.Controllers
{
    [Route("places")]
    [ApiController]
    public class PlaceController : ControllerBase
    {
        private readonly IPlaceService _placeService;

        public PlaceController(IPlaceService placeService)
        {
            _placeService = placeService;
        }

        [HttpGet("top-rating")]
        public async Task<ActionResult<List<PlaceDTO>>> GetTopRatingPlaces(int limit = 5)
        {
            if(limit <= 0)
            {
                return BadRequest("Số lượng phải lớn hơn 0");
            }

            var places = await _placeService.GetTopRatePlace(limit);
            return Ok(places);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlaceDTO>> GetPlaceById(int id)
        {
            var place = await _placeService.GetPlaceByID(id);
            if (place == null)
            {
                return NotFound($"Không tìm thấy Place với Id {id}.");
            }
            return Ok(place);
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<List<PlaceDTO>>> GetAllPlaces()
        {
            var places = await _placeService.GetAllPlacesAsync();
            return Ok(places);
        }

        [HttpPost("add-place")]
        public async Task<ActionResult<Place>> AddPlace([FromBody] Place place)
        {
            if (place == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            var validationContext = new ValidationContext(place);
            var validationResult = new List<ValidationResult>();
            if (!Validator.TryValidateObject(place, validationContext, validationResult, true))
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            try
            {
                var newPlace = await _placeService.AddPlaceAsync(place);
                return CreatedAtAction(
                    nameof(GetPlaceById), // Tên action để lấy Place theo Id
                    new { id = newPlace.Id }, // Route values
                    newPlace // Dữ liệu trả về
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("upload-image/{placeId}")]
        public async Task<ActionResult<List<string>>> UploadImagePlace(int placeId,[FromForm] List<IFormFile> images)
        {
            if (images == null || images.Count == 0)
            {
                return BadRequest("Không có ảnh nào được tải lên haha");
            }

            const int MAX_IMAGE_COUNT = 10;
            if (images.Count > MAX_IMAGE_COUNT)
            {
                return BadRequest($"Số lượng ảnh tối đa là {MAX_IMAGE_COUNT}");
            }
            try
            {
                var imageUrls = await _placeService.UploadImagePlaceAsync(placeId, images);

                if(imageUrls == null || imageUrls.Count == 0)
                {
                    return BadRequest("Không có ảnh nào được tải lên");
                }

                return Ok(imageUrls);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
    }
}
