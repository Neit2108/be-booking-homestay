using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace HomestayBookingAPI.Services.StatisticsServices
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;
        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<StatisticsResponse> GetStatisticsAsync(string role, string userId)
        {
            try
            {
                // Xác định khoảng thời gian tuần hiện tại và tuần trước
                var today = DateTime.UtcNow;
                var thisWeekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday); // Thứ Hai của tuần này
                var thisWeekEnd = today; // Đến ngày hiện tại
                var lastWeekStart = thisWeekStart.AddDays(-7); // Thứ Hai tuần trước
                var lastWeekEnd = lastWeekStart.AddDays(6); // Chủ Nhật tuần trước

                // Khởi tạo query với điều kiện lọc theo vai trò
                IQueryable<Booking> bookingsQuery = _context.Bookings
                    .Include(b => b.Place)
                    .AsQueryable();

                // Lọc dữ liệu dựa trên vai trò
                if (role == "Admin")
                {
                    // Admin: Lấy tất cả bookings
                }
                else if (role == "Landlord")
                {
                    // Landlord: Chỉ lấy bookings của các place thuộc sở hữu của họ
                    bookingsQuery = bookingsQuery.Where(b => b.Place.OwnerId == userId);
                }
                else // Customer
                {
                    // Customer: Chỉ lấy bookings của chính họ
                    bookingsQuery = bookingsQuery.Where(b => b.UserId == userId);
                }

                // Lấy dữ liệu bookings đã lọc
                var thisWeekBookings = await bookingsQuery
                    .Where(b => b.StartDate >= thisWeekStart && b.StartDate <= thisWeekEnd)
                    .ToListAsync();

                var lastWeekBookings = await bookingsQuery
                    .Where(b => b.StartDate >= lastWeekStart && b.StartDate <= lastWeekEnd)
                    .ToListAsync();

                // Tính số liệu tuần hiện tại
                var totalUsers = thisWeekBookings.Sum(b => b.NumberOfGuests);
                var totalBookings = thisWeekBookings.Count;
                var totalRevenue = thisWeekBookings.Sum(b => b.TotalPrice); // TotalPrice là double
                var refundedBookings = thisWeekBookings.Count(b => b.Status == BookingStatus.Cancelled);

                // Tính số liệu tuần trước để so sánh
                var lastWeekUsers = lastWeekBookings.Sum(b => b.NumberOfGuests);
                var lastWeekTotalBookings = lastWeekBookings.Count;
                var lastWeekRevenue = lastWeekBookings.Sum(b => b.TotalPrice); // TotalPrice là double
                var lastWeekRefunded = lastWeekBookings.Count(b => b.Status == BookingStatus.Cancelled);

                // Tính phần trăm thay đổi
                var calculateChange = (double current, double previous) =>
                {
                    if (previous == 0) return new { Value = current, Percentage = current > 0 ? 100.0 : 0.0 };
                    var changeValue = current - previous;
                    var changePercentage = (changeValue / previous) * 100;
                    return new { Value = changeValue, Percentage = changePercentage };
                };

                var usersChange = calculateChange(totalUsers, lastWeekUsers);
                var bookingsChange = calculateChange(totalBookings, lastWeekTotalBookings);
                var revenueChange = calculateChange(totalRevenue, lastWeekRevenue);
                var refundedChange = calculateChange(refundedBookings, lastWeekRefunded);

                // Dữ liệu cho biểu đồ Line Chart (Online Bookings theo tháng)
                var allBookings = await bookingsQuery.ToListAsync();
                var monthlyBookings = new double[12];
                foreach (var booking in allBookings)
                {
                    var month = booking.StartDate.Month - 1; // 0-11
                    monthlyBookings[month]++;
                }

                // Dữ liệu cho Doughnut Chart (Doanh thu theo trạng thái)
                var earningsByStatus = await bookingsQuery
                    .GroupBy(b => b.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Total = g.Sum(b => b.TotalPrice) // TotalPrice là double
                    })
                    .ToListAsync();

                var earningsData = new
                {
                    Confirmed = earningsByStatus.FirstOrDefault(e => e.Status == BookingStatus.Confirmed)?.Total ?? 0,
                    Pending = earningsByStatus.FirstOrDefault(e => e.Status == BookingStatus.Pending)?.Total ?? 0,
                    Cancelled = earningsByStatus.FirstOrDefault(e => e.Status == BookingStatus.Cancelled)?.Total ?? 0
                };

                // Trả về kết quả
                return new StatisticsResponse
                {
                    TotalUsers = totalUsers,
                    TotalBookings = totalBookings,
                    TotalRevenue = totalRevenue,
                    RefundedBookings = refundedBookings,
                    UsersChange = usersChange,
                    BookingsChange = bookingsChange,
                    RevenueChange = revenueChange,
                    RefundedChange = refundedChange,
                    LineChartData = new ChartData
                    {
                        Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" },
                        Datasets = new[]
                        {
                    new ChartDataset
                    {
                        Label = "Online orders",
                        Data = monthlyBookings, // monthlyBookings là double[]
                        BorderColor = new[] { "#F59E0B" }, // Đổi thành string[]
                        BackgroundColor = new[] { "#F59E0B" }, // Đổi thành string[]
                        Fill = false,
                        Tension = 0.4
                    }
                }
                    },
                    DoughnutChartData = new ChartData
                    {
                        Labels = new[] { "Confirmed", "Pending", "Cancelled" },
                        Datasets = new[]
                        {
                    new ChartDataset
                    {
                        Data = new[] { earningsData.Confirmed, earningsData.Pending, earningsData.Cancelled }, // earningsData là double
                        BackgroundColor = new[] { "#10B981", "#F59E0B", "#EF4444" }, // Đổi thành string[]
                        HoverBackgroundColor = new[] { "#059669", "#D97706", "#DC2626" },
                        BorderWidth = 0
                    }
                }
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thống kê: {ex.Message}", ex);
            }
        }
    }
}
