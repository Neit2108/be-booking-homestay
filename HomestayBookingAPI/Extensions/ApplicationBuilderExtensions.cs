using Hangfire;
using HomestayBookingAPI.Services.BookingLifecycleServices;
using HomestayBookingAPI.Services.NotifyServices;
using HomestayBookingAPI.Services.TopRatePlaceServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomestayBookingAPI.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static WebApplication ConfigureApp(this WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseStaticFiles();
            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire");

            app.MapControllers();

            return app;
        }

        public static void ConfigureHangfireJobs(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
                recurringJobManager.AddOrUpdate(
                    "update-notification-status",
                    () => scope.ServiceProvider.GetRequiredService<INotifyService>().UpdateNotificationStatusAsync(),
                    "*/5 * * * *"); // Tạo thông báo mỗi 5p

                recurringJobManager.AddOrUpdate(
                    "update-top-rated-places",
                    () => scope.ServiceProvider.GetRequiredService<ITopRateService>().UpdateTopRateAsync(5),
                    "*/5 * * * *"); // Cập nhật top 5 địa điểm mỗi 5p

                recurringJobManager.AddOrUpdate(
                    "process-completed-bookings",
                    () => scope.ServiceProvider.GetRequiredService<IBookingLifecycleService>().ProcessCompletedBookingsAsync(),
                    "0 0 * * *"); // Xử lý các booking đã hoàn thành mỗi ngày lúc 0h sáng

                recurringJobManager.AddOrUpdate(
                    "cleanup-old-bookings",
                    () => scope.ServiceProvider.GetRequiredService<IBookingLifecycleService>().CleanupOldBookingsAsync(),
                    "0 0 * * *"); // Xóa các booking cũ (từ 30 ngày) mỗi ngày lúc 0h sáng
            }
        }
    }
}