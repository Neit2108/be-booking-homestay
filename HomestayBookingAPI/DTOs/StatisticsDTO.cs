namespace HomestayBookingAPI.DTOs
{
    public class StatisticsResponse
    {
        public int TotalUsers { get; set; }
        public int TotalBookings { get; set; }
        public double TotalRevenue { get; set; }
        public int RefundedBookings { get; set; }
        public dynamic UsersChange { get; set; }
        public dynamic BookingsChange { get; set; }
        public dynamic RevenueChange { get; set; }
        public dynamic RefundedChange { get; set; }
        public ChartData LineChartData { get; set; }
        public ChartData DoughnutChartData { get; set; }
    }

    public class ChartData
    {
        public string[] Labels { get; set; }
        public ChartDataset[] Datasets { get; set; }
    }

    public class ChartDataset
    {
        public string Label { get; set; }
        public double[] Data { get; set; }
        public string[] BorderColor { get; set; }
        public string[] BackgroundColor { get; set; }
        public string[] HoverBackgroundColor { get; set; }
        public int BorderWidth { get; set; }
        public bool Fill { get; set; }
        public double Tension { get; set; }
    }
}
