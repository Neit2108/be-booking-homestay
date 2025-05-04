using Microsoft.Extensions.DependencyInjection;

namespace HomestayBookingAPI.Configuration
{
    public static class SwaggerConfig
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddOpenApi();

            return services;
        }
    }
}