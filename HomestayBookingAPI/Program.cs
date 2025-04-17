using Hangfire;
using Hangfire.PostgreSql;
using HomestayBookingAPI.Data;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.AuthService;
using HomestayBookingAPI.Services.BookingServices;
using HomestayBookingAPI.Services.EmailServices;
using HomestayBookingAPI.Services.ImageServices;
using HomestayBookingAPI.Services.JwtServices;
using HomestayBookingAPI.Services.NotifyServices;
using HomestayBookingAPI.Services.OwnerServices;
using HomestayBookingAPI.Services.PlaceServices;
using HomestayBookingAPI.Services.ProfileServices;
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(option =>
{
    option.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
    }));
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1;
});
GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 }); // gửi lại 3 lần nếu thất bại

var configuration = builder.Configuration;

var secretKey = Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer( options =>
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

builder.Services.AddAuthorization();

// Thêm service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<ITopRateService, TopRateService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<INotifyService, NotifyService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITestCaseService, TestCaseService>();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders(); 
    logging.AddConsole(); 
    logging.AddDebug();   
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    await SeedRole.InitializeRolesAndAdmin(roleManager, userManager);
} // tạo role và admin mặc định

app.UseStaticFiles();
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


//app.UseHangfireDashboard("/hangfire", new DashboardOptions
//{
//    Authorization = new[] { new AuthorizationFilter() }, 
//});
app.UseHangfireDashboard("/hangfire");
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate(
        "update-notification-status",
        () => scope.ServiceProvider.GetRequiredService<INotifyService>().UpdateNotificationStatusAsync(),
        "*/5 * * * *");

    recurringJobManager.AddOrUpdate(
        "update-top-rated-places",
        () => scope.ServiceProvider.GetRequiredService<ITopRateService>().UpdateTopRateAsync(5),
        "*/5 * * * *");
}

app.MapControllers();

app.Run();
