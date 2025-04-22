using HomestayBookingAPI.Data;
using HomestayBookingAPI.Services.ImageServices;
using HomestayBookingAPI.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomestayBookingAPI.Controllers
{
    [Route("uploads")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public ImageController(IImageService imageService, IUserService userService, ApplicationDbContext context)
        {
            _imageService = imageService;
            _userService = userService;
            _context = context;
        }

        [HttpPost("upload-image")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(id))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
            }

            var user = await _userService.GetUserByID(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            var imageUrl = await _imageService.UploadImageAsync(file, "avatars");
            if (imageUrl == null)
            {
                return BadRequest(new { message = "Không thể tải ảnh lên" });
            }

            user.AvatarUrl = imageUrl;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                avatarUrl = user.AvatarUrl
            });
        }

        [HttpPut("update-image")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> UpdateImage([FromForm] IFormFile file)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(id))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
            }

            var user = await _userService.GetUserByID(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            var imageUrl = await _imageService.UpdateImageAsync(user.AvatarUrl, file, "avatars");
            if (imageUrl == null)
            {
                return BadRequest(new { message = "Không thể cập nhật ảnh" });
            }
            user.AvatarUrl = imageUrl;
            await _context.SaveChangesAsync();
            return Ok(new
            {
                avatarUrl = user.AvatarUrl,
            });
        }

        [HttpDelete("delete-image")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> DeleteImage()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(id))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
            }
            var user = await _userService.GetUserByID(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }
            var result = await _imageService.DeleteImageAsync(user.AvatarUrl);
            if (!result)
            {
                return BadRequest(new { message = "Không thể xóa ảnh" });
            }
            user.AvatarUrl = null;
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Xóa ảnh thành công"
            });
        }
    }
}
