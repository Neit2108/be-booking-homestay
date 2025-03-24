namespace HomestayBookingAPI.Services.ImageServices
{
    public interface IImageService
    {
        Task<string?> UploadImageAsync(IFormFile file);
        Task<string?> UpdateImageAsync(string? oldImageUrl, IFormFile file);
        Task<bool> DeleteImageAsync(string? imageUrl);
    }
}
