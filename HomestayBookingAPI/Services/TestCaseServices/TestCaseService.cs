using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.TestCase;
using HomestayBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HomestayBookingAPI.Services.TestCaseServices
{
    public class TestCaseService : ITestCaseService
    {
        private readonly ApplicationDbContext _context;
        public TestCaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TestCaseResponse> CreateTestcaseAsync(TestCaseRequest testCaseRequest)
        {
            var testCase = new TestCaseModel
            {
                TestId = testCaseRequest.TestId,
                Name = testCaseRequest.Name,
                Target = testCaseRequest.Target,
                ImplementationSteps = testCaseRequest.ImplementationSteps,
                Input = testCaseRequest.Input,
                ExpectedOutput = testCaseRequest.ExpectedOutput,
                Status = testCaseRequest.Status,
                Note = testCaseRequest.Note,
                CreatedAt = DateTime.UtcNow
            };
            await _context.TestCases.AddAsync(testCase);
            await _context.SaveChangesAsync();
            return new TestCaseResponse
            {
                Id = testCase.Id,
                TestId = testCase.TestId,
                Name = testCase.Name,
                Target = testCase.Target,
                ImplementationSteps = testCase.ImplementationSteps,
                Input = testCase.Input,
                ExpectedOutput = testCase.ExpectedOutput,
                Status = testCase.Status,
                Note = testCase.Note,
                CreatedAt = testCase.CreatedAt
            };
        }

        public async Task<IEnumerable<TestCaseResponse>> GetAllTestCasesAsync()
        {
            var testCases = await _context.TestCases.ToListAsync();
            return testCases.Select(tc => new TestCaseResponse
            {
                Id = tc.Id,
                TestId = tc.TestId,
                Name = tc.Name,
                Target = tc.Target,
                ImplementationSteps = tc.ImplementationSteps,
                Input = tc.Input,
                ExpectedOutput = tc.ExpectedOutput,
                Status = tc.Status,
                Note = tc.Note,
                CreatedAt = tc.CreatedAt
            });
        }

        public async Task<TestCaseResponse> GetTestCaseByIdAsync(int id)
        {
            var testCase = await _context.TestCases.FindAsync(id);
            if (testCase == null)
            {
                return null;
            }
            return new TestCaseResponse
            {
                Id = testCase.Id,
                TestId = testCase.TestId,
                Name = testCase.Name,
                Target = testCase.Target,
                ImplementationSteps = testCase.ImplementationSteps,
                Input = testCase.Input,
                ExpectedOutput = testCase.ExpectedOutput,
                Status = testCase.Status,
                Note = testCase.Note,
                CreatedAt = testCase.CreatedAt
            };
        }
    }
    
}
