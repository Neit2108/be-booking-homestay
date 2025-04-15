using HomestayBookingAPI.DTOs.TestCase;

namespace HomestayBookingAPI.Services.TestCaseServices
{
    public interface ITestCaseService
    {
        Task<IEnumerable<TestCaseResponse>> GetAllTestCasesAsync();
        Task<TestCaseResponse> GetTestCaseByIdAsync(int id);
        Task<TestCaseResponse> CreateTestcaseAsync(TestCaseRequest testCaseRequest);
    }
}
