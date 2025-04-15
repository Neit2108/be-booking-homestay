using HomestayBookingAPI.DTOs.TestCase;
using HomestayBookingAPI.Services.TestCaseServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomestayBookingAPI.Controllers
{
    [Route("testcase")]
    [ApiController]
    public class TestCaseController : ControllerBase
    {
        private readonly ITestCaseService _testCaseService;
        public TestCaseController(ITestCaseService testCaseService)
        {
            _testCaseService = testCaseService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllTestCases()
        {
            var testCases = await _testCaseService.GetAllTestCasesAsync();
            return Ok(testCases);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTestCaseById(int id)
        {
            var testCase = await _testCaseService.GetTestCaseByIdAsync(id);
            if (testCase == null)
            {
                return NotFound($"Test case with ID {id} not found");
            }
            return Ok(testCase);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTestCase([FromBody] TestCaseRequest testCaseRequest)
        {
            if (testCaseRequest == null)
            {
                return BadRequest("Invalid test case request");
            }
            var createdTestCase = await _testCaseService.CreateTestcaseAsync(testCaseRequest);
            return CreatedAtAction(nameof(GetTestCaseById), new { id = createdTestCase.Id }, createdTestCase);
        }
    }
}
