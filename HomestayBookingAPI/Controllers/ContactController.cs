using HomestayBookingAPI.DTOs.Contact;
using HomestayBookingAPI.Services.ContactServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomestayBookingAPI.Controllers
{
    [Route("contacts")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;
        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet("contact/{id}")]
        public async Task<IActionResult> GetContactById(int id)
        {
            var contact = await _contactService.GetContactByIdAsync(id);
            if (contact == null)
            {
                return NotFound(new { message = "Contact not found" });
            }
            return Ok(contact);
        }

        [HttpPost("add-contact")]
        public async Task<IActionResult> CreateContact([FromBody] ContactRequest contactRequest)
        {
            if (contactRequest == null)
            {
                return BadRequest(new { message = "Invalid contact request" });
            }
            var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _contactService.CreateContactAsync(contactRequest, currentId);
            return CreatedAtAction(nameof(GetContactById), new { id = result.Id }, result);
        }
    }
}
