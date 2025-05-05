namespace HomestayBookingAPI.DTOs.Voucher
{
    public class VoucherRequestForCreate
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int UsageCount { get; set; }
        public int MaxUsage {  get; set; }
        public double Discount { get; set; }
        public DateTime From { get; set; }
        public DateTime To {  get; set; }
    }
}
