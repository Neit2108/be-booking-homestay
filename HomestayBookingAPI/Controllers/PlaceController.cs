﻿using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.Place;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.PlaceServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HomestayBookingAPI.Controllers
{
    [Route("places")]
    [ApiController]
    public class PlaceController : ControllerBase
    {
        private readonly IPlaceService _placeService;
        private readonly ApplicationDbContext _context;

        public PlaceController(IPlaceService placeService, ApplicationDbContext context)
        {
            _placeService = placeService;
            _context = context;
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

        [HttpGet("place-details/{id}")]
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

        [HttpGet("get-all-for-landlord/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
        public async Task<ActionResult<List<PlaceDTO>>> GetAllPlacesForLandlord(string id)
        {
            
            var places = await _placeService.GetAllPlacesOfLandlord(id);
            return Ok(places);
        }

        [HttpGet("get-same-category/{id}")]
        public async Task<ActionResult<List<PlaceDTO>>> GetSameCategoryPlaces(int id)
        {
            var sameCategoryPlaces = await _placeService.GetSameCategoryPlaces(id);
            if (sameCategoryPlaces == null || sameCategoryPlaces.Count == 0)
            {
                return NotFound($"Không tìm thấy danh sách địa điểm cùng loại với Id {id}.");
            }
            return Ok(sameCategoryPlaces);
        }

        [HttpPost("add-place")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
        public async Task<IActionResult> AddPlace([FromForm] PlaceRequest placeRequest)
        {
            
            try
            {
                var newPlace = await _placeService.AddPlaceAsync(placeRequest);
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

        [HttpGet("bulk")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Landlord")]
        public async Task<IActionResult> GetPlacesBulk([FromQuery] string ids)
        {
            if(string.IsNullOrEmpty(ids.ToString()))
            {
                return BadRequest("Không có id nào được cung cấp á");
            }
            var placeIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(id => int.Parse(id.Trim()))
                          .ToList();
            if (!placeIds.Any())
            {
                return BadRequest("Không có id nào được cung cấp");
            }
            try
            {
                var places = await _context.Places
                    .Where(p => placeIds.Contains(p.Id))
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name ?? "Unknown Place",
                        address = p.Address
                    })
                    .ToListAsync();
                return Ok(places);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách địa điểm: {ex.Message}");
            }
        }
    }
}
