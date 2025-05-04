using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomestayBookingAPI.Configuration
{
    public static class CloudinaryConfig
    {
        public static IServiceCollection AddCloudinaryConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var cloudinaryConfig = configuration.GetSection("Cloudinary");
            var cloudName = cloudinaryConfig["CloudName"];
            var apiKey = cloudinaryConfig["ApiKey"];
            var apiSecret = cloudinaryConfig["ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account);

            services.AddSingleton(cloudinary);

            return services;
        }
    }
}