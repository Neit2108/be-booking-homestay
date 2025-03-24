
using HomestayBookingAPI.Services.PlaceServices;
using HomestayBookingAPI.Services.UserServices;

namespace HomestayBookingAPI.Services.ImageServices
{
    public class ImageService : IImageService
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IUserService userService, IWebHostEnvironment webHostEnvironment, ILogger<ImageService> logger)
        {
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<bool> DeleteImageAsync(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }

        public async Task<string?> UpdateImageAsync(string? oldImageUrl, IFormFile file)
        {
            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, oldImageUrl.TrimStart('/'));
                if (File.Exists(oldFilePath))
                    File.Delete(oldFilePath);
            }

            return await UploadImageAsync(file);
        }

        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            if(file == null)
            {
                _logger.LogWarning("No file uploaded");
                return null;
            }
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"https://localhost:7284/uploads/{fileName}";
        }

    }
}
