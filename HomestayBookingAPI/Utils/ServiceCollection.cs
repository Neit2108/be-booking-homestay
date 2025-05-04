using CloudinaryDotNet;
using Hangfire;
using Hangfire.PostgreSql;
using HomestayBookingAPI.Data;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.AuthService;
using HomestayBookingAPI.Services.BookingServices;
using HomestayBookingAPI.Services.CommentServices;
using HomestayBookingAPI.Services.ContactServices;
using HomestayBookingAPI.Services.EmailServices;
using HomestayBookingAPI.Services.ImageServices;
using HomestayBookingAPI.Services.JwtServices;
using HomestayBookingAPI.Services.NotifyServices;
using HomestayBookingAPI.Services.OwnerServices;
using HomestayBookingAPI.Services.PaymentServices;
using HomestayBookingAPI.Services.PlaceServices;
using HomestayBookingAPI.Services.ProfileServices;
using HomestayBookingAPI.Services.StatisticsServices;
using HomestayBookingAPI.Services.TestCaseServices;
using HomestayBookingAPI.Services.TopRatePlaceServices;
using HomestayBookingAPI.Services.UserServices;
using HomestayBookingAPI.Services.VoucherServices;
using HomestayBookingAPI.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace HomestayBookingAPI.Utils {
    public static class ServiceCollection
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            var cloudinaryConfig = configuration.GetSection("Cloudinary");
            var account = new Account(
            cloudinaryConfig["CloudName"],
            cloudinaryConfig["ApiKey"],
            cloudinaryConfig["ApiSecret"]
            );
            services.AddSingleton(new Cloudinary(account));
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
            });
            var secretKey = Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = configuration["JwtSettings:Audience"],

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),

                    ValidateLifetime = true,
                    RoleClaimType = ClaimTypes.Role
                };
            });

            services.Configure<VNPayConfig>(configuration.GetSection("VNPay"));

            // Add services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IPlaceService, PlaceService>();
            services.AddScoped<ITopRateService, TopRateService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IOwnerService, OwnerService>();
            services.AddScoped<IVoucherService, VoucherService>();
            services.AddScoped<INotifyService, NotifyService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ITestCaseService, TestCaseService>();
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IVNPayService, VNPayService>();
            services.AddScoped<HttpClient>();

            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
            });

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddOpenApi();

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
        }
    }
}