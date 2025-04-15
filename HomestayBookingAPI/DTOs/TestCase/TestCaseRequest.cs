namespace HomestayBookingAPI.DTOs.TestCase
{
    public class TestCaseRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Target { get; set; }
        public string ImplementationSteps { get; set; }
        public string Input { get; set; }
        public string ExpectedOutput { get; set; }
        public string Status { get; set; }
        public string? Note { get; set; }
    }
}
