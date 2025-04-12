using HomestayBookingAPI.Data;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using HomestayBookingAPI.Services.EmailServices;
using HomestayBookingAPI.Services.JwtServices;
using HomestayBookingAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace HomestayBookingAPI.Services.NotifyServices
{
    public class NotifyService : INotifyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly string _baseUrl;
        private readonly IJwtService _jwtService;

        public NotifyService(ApplicationDbContext context, IEmailService emailService, IConfiguration config, IJwtService jwtService)
        {
            _emailService = emailService;
            _context = context;
            _baseUrl = config["App:BaseUrl"] ?? "https://localhost:5173";
            _jwtService = jwtService;
        }

        public async Task CreateNewBookingNotificationAsync(Booking booking, bool sendEmail = true)
        {
            var customer = await _context.Users.FindAsync(booking.UserId);
            var place = await _context.Places.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == booking.PlaceId);
            var landlord = place?.Owner;

            if(customer == null || place == null || landlord == null)
            {
                throw new Exception("Customer or Place not found");
            }

            var customerToken = _jwtService.GenerateActionToken(customer.Id, NotificationType.BookingConfirmation.ToString() ,booking.Id, "Customer");
            //var landlordToken = _jwtService.GenerateEmailConfirmationToken(landlord, "Landlord", booking.Id);


            var customerNotify = new Notification
            {
                RecipientId = customer.Id,
                SenderId = "system",
                BookingId = booking.Id,
                Type = NotificationType.ConfirmInfo,
                Title = "Xác nhận thông tin đặt phòng",
                Message = $"Your booking request for {place.Name} from {booking.StartDate.ToShortDateString()} to {booking.EndDate.ToShortDateString()} has been submitted.",
                Url = $"{_baseUrl}/auth/verify-action/{customerToken}",
                Status = NotificationStatus.Pending,
            };

            var lanlordNotify = new Notification
            {
                RecipientId = landlord.Id,
                SenderId = "system",
                BookingId = booking.Id,
                Type = NotificationType.BookingRequest,
                Title = "Yêu cầu đặt phòng",
                Message = $"You have a new booking request from {customer.FullName} for {place.Name} from {booking.StartDate.ToShortDateString()} to {booking.EndDate.ToShortDateString()}.",
                Url = $"{_baseUrl}/landlord/booking/{booking.Id}",
                Status = NotificationStatus.Pending,
            };

            _context.Notifications.AddRange(customerNotify, lanlordNotify);
            await _context.SaveChangesAsync();

            if(sendEmail)
            {
                await SendBookingEmailAsync(booking.Id);
            }

        }

        

        public async Task SendBookingEmailAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Place)
                .ThenInclude(p => p.Owner)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                throw new Exception("Không tìm thấy booking");
                return;
            }
            var customer = booking.User;
            var place = booking.Place;
            var landlord = place?.Owner;

            if (customer == null || place == null || landlord == null)
            {
                throw new Exception("Không tìm thấy khách hàng hoặc địa điểm");
            }

            var customerNotify = await _context.Notifications
                .FirstOrDefaultAsync(n => n.BookingId == booking.Id && n.RecipientId == customer.Id && n.Type == NotificationType.ConfirmInfo);
            var lanlordNotify = await _context.Notifications
                .FirstOrDefaultAsync(n => n.BookingId == booking.Id && n.RecipientId == landlord.Id && n.Type == NotificationType.BookingRequest);

            if (customerNotify == null || lanlordNotify == null)
            {
                throw new Exception("Không tìm thấy thông báo");
            }

            try
            {
                var customerEmail = TemplateMail.BookingConfirmationForCustomer(booking, customerNotify.Url);
                var landlordEmail = TemplateMail.BookingRequestForLanlord(booking, lanlordNotify.Url);

                await _emailService.SendEmailAsync(customer.Email, "Xác nhận thông tin đặt phòng", customerEmail);
                customerNotify.Status = NotificationStatus.Sent;
                await _emailService.SendEmailAsync(landlord.Email, "Yêu cầu đặt phòng", landlordEmail);
                lanlordNotify.Status = NotificationStatus.Sent;
            }
            catch (Exception ex)
            {
                customerNotify.Status = NotificationStatus.Failed;
                lanlordNotify.Status = NotificationStatus.Failed;
                await _context.SaveChangesAsync();
                throw new Exception("Không thể gửi email", ex);
            }

            await _context.SaveChangesAsync();
        }


    }
}
