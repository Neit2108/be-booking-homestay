using Hangfire;
using Hangfire.PostgreSql;
using HomestayBookingAPI.Services.NotifyServices;
using HomestayBookingAPI.Services.TopRatePlaceServices;
using HomestayBookingAPI.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomestayBookingAPI.Configuration
{
    public static class HangfireConfig
    {
        public static IServiceCollection AddHangfireConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
                }));

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 1;
            });

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 });

            return services;
        }
    }
}