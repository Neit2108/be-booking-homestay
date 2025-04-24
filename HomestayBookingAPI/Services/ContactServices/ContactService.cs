using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Contact;
using HomestayBookingAPI.Models;

namespace HomestayBookingAPI.Services.ContactServices
{
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _context;

        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ContactResponse> CreateContactAsync(ContactRequest contactRequest, string? senderId)
        {
            try
            {

                var contact = new Contact
                {
                    SenderName = contactRequest.SenderName,
                    SenderEmail = contactRequest.SenderEmail,
                    SenderPhone = contactRequest.SenderPhone,
                    Title = contactRequest.Title,
                    Message = contactRequest.Message
                };
                if (String.IsNullOrEmpty(senderId))
                {
                    contact.SenderId = senderId;
                }
                await _context.Contacts.AddAsync(contact);
                await _context.SaveChangesAsync();

                return new ContactResponse
                {
                    Id = contact.Id,
                    SenderId = contact.SenderId,
                    //SenderName = contact.SenderName,
                    //SenderEmail = contact.SenderEmail,
                    //SenderPhone = contact.SenderPhone,
                    Title = contact.Title,
                    Message = contact.Message
                };
            }
            catch (Exception ex)
            {

                throw new Exception("An error occurred while creating the contact", ex);
            }
        }

        public async Task<ContactResponse> GetContactByIdAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if(contact == null)
            {
                throw new KeyNotFoundException($"Contact with ID {id} not found");
            }
            return new ContactResponse
            {
                Id = contact.Id,
                SenderId = contact.SenderId,
                //SenderName = contact.SenderName,
                //SenderEmail = contact.SenderEmail,
                Title = contact.Title,
                Message = contact.Message
            };
        }
    }
}
