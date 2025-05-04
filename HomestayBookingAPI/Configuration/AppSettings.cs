using HomestayBookingAPI.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomestayBookingAPI.Configuration
{
    public static class AppSettings
    {
        public static IServiceCollection AddApplicationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<VNPayConfig>(configuration.GetSection("VNPay"));

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
            });

            services.AddHttpClient();

            return services;
        }
    }
}