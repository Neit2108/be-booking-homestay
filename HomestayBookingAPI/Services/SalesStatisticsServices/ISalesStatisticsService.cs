using HomestayBookingAPI.DTOs.SalesStatistics;

namespace HomestayBookingAPI.Services.SalesStatisticsServices
{
    public interface ISalesStatisticsService
    {
        Task<SalesStatisticsDTO> GetLandlordSalesStatisticsAsync(
        string landlordId,
        DateTime? startDate = null,
        DateTime? endDate = null);

        Task<SalesStatisticsDTO> GetAdminSalesStatisticsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}
