using HomestayBookingAPI.DTOs.Contact;

namespace HomestayBookingAPI.Services.ContactServices
{
    public interface IContactService
    {
        Task<ContactResponse> CreateContactAsync(ContactRequest contactRequest, string? senderId);
        Task<ContactResponse> GetContactByIdAsync(int id);
    }
}
