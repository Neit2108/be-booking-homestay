using HomestayBookingAPI.Configuration;
using HomestayBookingAPI.Extensions;
using HomestayBookingAPI.Utils;
using Microsoft.AspNetCore.Identity;
using HomestayBookingAPI.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(prefix: "");

builder.Services.AddApplicationSettings(builder.Configuration);
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddCloudinaryConfiguration(builder.Configuration);
builder.Services.AddHangfireConfiguration(builder.Configuration);
builder.Services.AddCorsConfiguration();
builder.Services.AddApplicationServices();
builder.Services.AddControllers();
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

app.ConfigureApp(app.Environment);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    await SeedRole.InitializeRolesAndAdmin(roleManager, userManager);
}

app.ConfigureHangfireJobs();

app.Run();