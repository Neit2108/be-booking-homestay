namespace HomestayBookingAPI.Services.ImageServices
{
    public interface IImageService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folder);
        Task<string?> UpdateImageAsync(string? oldImageUrl, IFormFile file, string folder);
        Task<bool> DeleteImageAsync(string? imageUrl);
    }
}
