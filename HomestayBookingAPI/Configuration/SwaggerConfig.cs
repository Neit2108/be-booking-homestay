using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace HomestayBookingAPI.Configuration
{
    public static class SwaggerConfig
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddOpenApi();

            services.AddSwaggerGen(options =>
            {
                // Thông tin cơ bản về API
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Homestay Booking API",
                    Version = "v1",
                    Description = "API cho hệ thống đặt phòng homestay toàn diện",
                    Contact = new OpenApiContact
                    {
                        Name = "Homestay Booking Team",
                        Email = "support@homestay-booking.com",
                        Url = new Uri("https://homestay-booking.com")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

                // Cấu hình JWT Bearer Authentication
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token JWT trong format: Bearer {token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });

                // Thêm XML comments từ project
                var xmlFile = Path.Combine(AppContext.BaseDirectory, "HomestayBookingAPI.xml");
                if (File.Exists(xmlFile))
                {
                    options.IncludeXmlComments(xmlFile);
                }

                // Hỗ trợ annotations từ Swashbuckle
                options.EnableAnnotations();
            });

            return services;
        }
    }
}