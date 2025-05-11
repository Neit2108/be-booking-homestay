using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.SalesStatistics;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using HomestayBookingAPI.Services.BookingServices;
using HomestayBookingAPI.Services.SalesStatisticsServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomestayBookingAPI.Services.SalesStatisticsServices
{
    public class SalesStatisticsService : ISalesStatisticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SalesStatisticsService> _logger;
        private readonly IConfiguration _configuration;
        private readonly double _commissionRate;

        public SalesStatisticsService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<SalesStatisticsService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;

            _commissionRate = _configuration.GetValue<double>("Commission:Rate", 0.18);
        }

        public async Task<SalesStatisticsDTO> GetLandlordSalesStatisticsAsync(
            string landlordId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(landlordId);
                if (user == null)
                {
                    _logger.LogError("Người dùng không tồn tại: {UserId}", landlordId);
                    throw new KeyNotFoundException($"Không tìm thấy người dùng với ID {landlordId}");
                }

                var isLandlord = await _userManager.IsInRoleAsync(user, "Landlord");
                if (!isLandlord)
                {
                    _logger.LogWarning("Người dùng {UserId} không phải là chủ nhà", landlordId);
                    throw new UnauthorizedAccessException("Người dùng không phải là chủ nhà");
                }

                var result = new SalesStatisticsDTO
                {
                    RevenueByMonth = new Dictionary<string, double>()
                };

                // Lấy tất cả các địa điểm của landlord
                var places = await _context.Places
                    .Where(p => p.OwnerId == landlordId)
                    .Select(p => p.Id)
                    .ToListAsync();

                if (!places.Any())
                {
                    _logger.LogInformation("Chủ nhà {LandlordId} không có địa điểm nào", landlordId);
                    return result;
                }

                // Lấy tất cả booking liên quan đến các địa điểm của landlord
                var query = _context.Bookings
                    .Where(b => places.Contains(b.PlaceId))
                    .AsQueryable();

                //lọc ngày
                if (startDate.HasValue)
                {
                    query = query.Where(b => b.EndDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(b => b.StartDate <= endDate.Value);
                }

                var bookings = await query
                    .Select(b => new
                    {
                        b.Id,
                        b.TotalPrice,
                        b.Status,
                        b.PaymentStatus,
                        b.CreatedAt,
                        b.StartDate,
                        b.EndDate
                    })
                    .ToListAsync();

                if (!bookings.Any())
                {
                    _logger.LogInformation("Không có đơn đặt phòng nào cho chủ nhà: {LandlordId}", landlordId);
                    return result;
                }

                // Phân loại và tính toán số liệu
                result.TotalBookings = bookings.Count;
                result.CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed);
                result.ConfirmedBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed);
                result.PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending);
                result.CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled);

                // Tính doanh thu từ các đơn đã hoàn thành thanh toán
                var completedAndPaidBookings = bookings
                    .Where(b => b.Status == BookingStatus.Completed && b.PaymentStatus == PaymentStatus.Paid)
                    .ToList();

                result.TotalRevenue = completedAndPaidBookings.Sum(b => b.TotalPrice);
                result.CommissionAmount = result.TotalRevenue * _commissionRate;
                result.ActualSales = result.TotalRevenue - result.CommissionAmount;

                // Thống kê doanh thu theo tháng
                var revenueByMonth = completedAndPaidBookings
                    .GroupBy(b => new { b.EndDate.Year, b.EndDate.Month })
                    .Select(g => new
                    {
                        YearMonth = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Revenue = g.Sum(b => b.TotalPrice)
                    })
                    .OrderBy(x => x.YearMonth)
                    .ToList();

                foreach (var item in revenueByMonth)
                {
                    result.RevenueByMonth[item.YearMonth] = item.Revenue;
                }

                _logger.LogInformation("Thống kê doanh thu cho chủ nhà {LandlordId}: Tổng {TotalRevenue}, Thực nhận {ActualSales}",
                    landlordId, result.TotalRevenue, result.ActualSales);

                return result;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && ex is not UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê doanh thu cho chủ nhà {LandlordId}", landlordId);
                throw new Exception($"Lỗi khi lấy thống kê doanh thu: {ex.Message}", ex);
            }
        }

        public async Task<SalesStatisticsDTO> GetAdminSalesStatisticsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                // Chuẩn bị kết quả
                var result = new SalesStatisticsDTO
                {
                    RevenueByMonth = new Dictionary<string, double>()
                };

                // Lấy tất cả booking trong hệ thống
                var query = _context.Bookings.AsQueryable();

                // Áp dụng lọc ngày
                if (startDate.HasValue)
                {
                    query = query.Where(b => b.EndDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(b => b.StartDate <= endDate.Value);
                }

                // Lấy dữ liệu từ database
                var bookings = await query
                    .Select(b => new
                    {
                        b.Id,
                        b.TotalPrice,
                        b.Status,
                        b.PaymentStatus,
                        b.CreatedAt,
                        b.StartDate,
                        b.EndDate
                    })
                    .ToListAsync();

                if (!bookings.Any())
                {
                    _logger.LogInformation("Không có đơn đặt phòng nào trong hệ thống");
                    return result;
                }

                // Phân loại và tính toán số liệu
                result.TotalBookings = bookings.Count;
                result.CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed);
                result.ConfirmedBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed);
                result.PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending);
                result.CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled);

                // Tính doanh thu từ các đơn đã hoàn thành thanh toán
                var completedAndPaidBookings = bookings
                    .Where(b => b.Status == BookingStatus.Completed && b.PaymentStatus == PaymentStatus.Paid)
                    .ToList();

                result.TotalRevenue = completedAndPaidBookings.Sum(b => b.TotalPrice);
                result.CommissionAmount = result.TotalRevenue * _commissionRate;
                result.ActualSales = result.TotalRevenue; // Đối với admin, doanh thu thực tế là toàn bộ

                // Thống kê doanh thu theo tháng
                var revenueByMonth = completedAndPaidBookings
                    .GroupBy(b => new { b.EndDate.Year, b.EndDate.Month })
                    .Select(g => new
                    {
                        YearMonth = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Revenue = g.Sum(b => b.TotalPrice)
                    })
                    .OrderBy(x => x.YearMonth)
                    .ToList();

                foreach (var item in revenueByMonth)
                {
                    result.RevenueByMonth[item.YearMonth] = item.Revenue;
                }

                _logger.LogInformation("Thống kê doanh thu toàn hệ thống: Tổng {TotalRevenue}, Hoa hồng {CommissionAmount}",
                    result.TotalRevenue, result.CommissionAmount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê doanh thu toàn hệ thống");
                throw new Exception($"Lỗi khi lấy thống kê doanh thu: {ex.Message}", ex);
            }
        }
    }
}