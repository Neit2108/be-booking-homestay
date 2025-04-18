using HomestayBookingAPI.DTOs;

namespace HomestayBookingAPI.Services.StatisticsServices
{
    public interface IStatisticsService
    {
        Task<StatisticsResponse> GetStatisticsAsync(string role, string id);
    }
}
