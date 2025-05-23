﻿using HomestayBookingAPI.Services.AuthService;
using HomestayBookingAPI.Services.BookingLifecycleServices;
using HomestayBookingAPI.Services.BookingServices;
using HomestayBookingAPI.Services.CommentServices;
using HomestayBookingAPI.Services.ContactServices;
using HomestayBookingAPI.Services.EmailServices;
using HomestayBookingAPI.Services.FavouriteServices;
using HomestayBookingAPI.Services.ImageServices;
using HomestayBookingAPI.Services.JwtServices;
using HomestayBookingAPI.Services.NotifyServices;
using HomestayBookingAPI.Services.OwnerServices;
using HomestayBookingAPI.Services.PaymentServices;
using HomestayBookingAPI.Services.PlaceServices;
using HomestayBookingAPI.Services.ProfileServices;
using HomestayBookingAPI.Services.PromotionServices;
using HomestayBookingAPI.Services.SalesStatisticsServices;
using HomestayBookingAPI.Services.TestCaseServices;
using HomestayBookingAPI.Services.TopRatePlaceServices;
using HomestayBookingAPI.Services.UserServices;
using HomestayBookingAPI.Services.VoucherServices;
using HomestayBookingAPI.Services.WalletServices;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace HomestayBookingAPI.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register all services
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
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IVNPayService, VNPayService>();
            services.AddScoped<IBookingLifecycleService, BookingLifecycleService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ISalesStatisticsService, SalesStatisticsService>();
            services.AddScoped<IFavouriteService, FavouriteService>();
            services.AddScoped<HttpClient>();

            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
            });

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            return services;
        }
    }
}