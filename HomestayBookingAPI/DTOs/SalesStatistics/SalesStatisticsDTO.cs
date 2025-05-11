namespace HomestayBookingAPI.DTOs.SalesStatistics
{
    public class SalesStatisticsDTO
    {
        public double TotalRevenue { get; set; } // Tổng 
        public double ActualSales { get; set; } // Thực tế
        public int TotalBookings { get; set; } 
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public double CommissionAmount { get; set; } // hoa hồng
        public Dictionary<string, double> RevenueByMonth { get; set; }
    }
}
