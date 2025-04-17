
using HomestayBookingAPI.Services.PlaceServices;
using HomestayBookingAPI.Services.UserServices;
using System.Net.Http.Headers;
using System.Text.Json;

namespace HomestayBookingAPI.Services.ImageServices
{
    public enum ImageType
    {
        Place,
        User
    }
    public class ImageService : IImageService
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ImageService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _imgurClientId;
        private readonly string _homestayAlbumId;
        private readonly string _userAlbumId;
        private const string IMGUR_UPLOAD_URL = "https://api.imgur.com/3/image";

        public ImageService(IUserService userService, IWebHostEnvironment webHostEnvironment, ILogger<ImageService> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _httpClient = httpClient;
            _imgurClientId = configuration["Imgur:ClientId"];
            _homestayAlbumId = configuration["Imgur:PlaceAlbumId"];
            _userAlbumId = configuration["Imgur:UserAlbumId"];
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
            if (file == null)
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


        //public async Task<string?> UploadImageAsync(IFormFile file, ImageType imageType)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        _logger.LogWarning("No file uploaded or file is empty");
        //        return null;
        //    }

        //    try
        //    {
        //        // Chọn Album ID dựa trên loại ảnh
        //        var albumId = imageType == ImageType.Place ? _homestayAlbumId : _userAlbumId;
        //        if (string.IsNullOrEmpty(albumId))
        //        {
        //            _logger.LogWarning("Album ID is not configured for {ImageType}", imageType);
        //            return null;
        //        }

        //        // Chuẩn bị nội dung ảnh để gửi lên Imgur
        //        using var content = new MultipartFormDataContent();
        //        using var stream = file.OpenReadStream();
        //        using var fileContent = new StreamContent(stream);
        //        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        //        content.Add(fileContent, "image", file.FileName);
        //        content.Add(new StringContent(albumId), "album"); // Thêm album ID vào request

        //        // Thêm header Authorization với Client ID
        //        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", _imgurClientId);

        //        // Gửi yêu cầu POST tới Imgur
        //        var response = await _httpClient.PostAsync(IMGUR_UPLOAD_URL, content);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            _logger.LogError("Imgur API error: StatusCode: {StatusCode}, Reason: {ReasonPhrase}, Content: {ErrorContent}",
        //                response.StatusCode, response.ReasonPhrase, errorContent);
        //            response.EnsureSuccessStatusCode(); // Sẽ ném ngoại lệ để vào catch
        //        }

        //        // Đọc phản hồi từ Imgur
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        using var jsonDoc = JsonDocument.Parse(responseContent);
        //        var imageUrl = jsonDoc.RootElement.GetProperty("data").GetProperty("link").GetString();

        //        if (string.IsNullOrEmpty(imageUrl))
        //        {
        //            _logger.LogError("Failed to retrieve image URL from Imgur response");
        //            return null;
        //        }

        //        _logger.LogInformation("Image uploaded successfully to Imgur: {ImageUrl} in album {AlbumId}", imageUrl, albumId);
        //        return imageUrl;
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        _logger.LogError(ex, "Failed to upload image to Imgur for {ImageType}", imageType);
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected error while uploading image to Imgur for {ImageType}", imageType);
        //        return null;
        //    }
        //}
    }
}
