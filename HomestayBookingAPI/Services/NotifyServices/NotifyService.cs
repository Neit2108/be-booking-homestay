using Hangfire;
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
        private readonly ILogger<NotifyService> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public NotifyService(ApplicationDbContext context, IEmailService emailService, IConfiguration config, IJwtService jwtService, ILogger<NotifyService> logger, IBackgroundJobClient backgroundJobClient)
        {
            _emailService = emailService;
            _context = context;
            _baseUrl = config["App:BaseUrl"] ?? "https://localhost:5173";
            _jwtService = jwtService;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
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

            var customerToken = _jwtService.GenerateActionToken(customer.Id, NotificationType.ConfirmInfo.ToString() ,booking.Id, "Customer");
            //var landlordToken = _jwtService.GenerateEmailConfirmationToken(landlord, "Landlord", booking.Id);
            var landlordToken = _jwtService.GenerateActionToken(landlord.Id, NotificationType.BookingRequest.ToString(), booking.Id, "Landlord");

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
                Url = $"{_baseUrl}/auth/verify-action/{landlordToken}",
                Status = NotificationStatus.Pending,
            };

            //_context.Notifications.AddRange(customerNotify, lanlordNotify);
            //await _context.SaveChangesAsync();

            try
            {
                var customerEmail = TemplateMail.BookingConfirmationForCustomer(booking, customerNotify.Url);
                var landlordEmail = TemplateMail.BookingRequestForLanlord(booking, lanlordNotify.Url);

                //await _emailService.SendEmailAsync(customer.Email, "Xác nhận thông tin đặt phòng", customerEmail);
                var customerJobId = _backgroundJobClient.Enqueue(() => _emailService.SendEmailAsync(customer.Email, "Xác nhận thông tin đặt phòng", customerEmail));
                customerNotify.JobId = customerJobId;
                //await _emailService.SendEmailAsync(landlord.Email, "Yêu cầu đặt phòng", landlordEmail);
                var landlordJobId = _backgroundJobClient.Enqueue(() => _emailService.SendEmailAsync(landlord.Email, "Yêu cầu đặt phòng", landlordEmail));
                lanlordNotify.JobId = landlordJobId;
            }
            catch (Exception ex)
            {
                customerNotify.Status = NotificationStatus.Failed;
                lanlordNotify.Status = NotificationStatus.Failed;
                throw new Exception("Không thể gửi email", ex);
            }

            _context.Notifications.AddRange(customerNotify,  lanlordNotify);
            await _context.SaveChangesAsync();

        }

        public async Task NotifyBookingStatusChangeAsync(int bookingId, bool isAccepted, string rejectReason = "Không xác định")
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Place)
                .ThenInclude(p => p.Owner)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                throw new Exception("Không tìm thấy booking");
            }

            var customer = booking.User;
            var place = booking.Place;
            var landlord = place?.Owner;

            if (customer == null || place == null || landlord == null)
            {
                throw new Exception("Không tìm thấy khách hàng hoặc địa điểm");
            }

            if(isAccepted)
            {
                booking.Status = BookingStatus.Confirmed;
            }
            else
            {
                booking.Status = BookingStatus.Cancelled;

            }

            var notificationType = isAccepted ? NotificationType.BookingConfirmation : NotificationType.BookingCancellation;
            var notificationMessage = isAccepted ? $"Đặt phòng tại {place.Name} từ {booking.StartDate} đến {booking.EndDate} thành công" :
                $"Đặt phòng tại {place.Name} từ {booking.StartDate} đến {booking.EndDate} không thành công. Yêu cầu của bạn bị từ chối";
            var notificationTitle = isAccepted ? "Đặt phòng thành công" : "Đặt phòng thất bại";

            var customerToken = _jwtService.GenerateActionToken(customer.Id, notificationType.ToString(), booking.Id, "Customer");
            if (customerToken == null)
            {
                throw new Exception("Không thể tạo token cho khách hàng");
            }
            var customerNotify = new Notification
            {
                RecipientId = customer.Id,
                SenderId = "system",
                BookingId = booking.Id,
                Type = notificationType,
                Title = notificationTitle,
                Message = notificationMessage,
                Url = $"{_baseUrl}/auth/verify-action/{customerToken}",
                Status = NotificationStatus.Pending,
            };

            _context.Notifications.Add(customerNotify);
            try
            {
                var customerEmail = TemplateMail.BookingStatusChangeEmail(booking, customerNotify.Url, isAccepted, rejectReason);
                //await _emailService.SendEmailAsync(customer.Email, notificationTitle, customerEmail);
                var customerJobId = _backgroundJobClient.Enqueue(() => _emailService.SendEmailAsync(customer.Email, notificationTitle, customerEmail));
                customerNotify.JobId = customerJobId;
            }
            catch (Exception ex)
            {
                customerNotify.Status = NotificationStatus.Failed;
                
                _logger.LogError(ex, "Failed to send email to customer for booking status change");
            }
            await _context.SaveChangesAsync();
        }


        public async Task UpdateNotificationStatusAsync()
        {
            var notifications = await _context.Notifications
                .Where(n => n.JobId != null && n.Status == NotificationStatus.Pending)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                try
                {
                    var job = JobStorage.Current.GetConnection().GetJobData(notification.JobId);
                    if (job != null && job.State == "Succeeded")
                    {
                        notification.Status = NotificationStatus.Sent;
                        _logger.LogInformation("Updated notification {NotificationId} to Sent for job {JobId}", notification.Id, notification.JobId);
                    }
                    else if (job != null && job.State == "Failed")
                    {
                        notification.Status = NotificationStatus.Failed;
                        _logger.LogInformation("Updated notification {NotificationId} to Failed for job {JobId}", notification.Id, notification.JobId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update status for notification {NotificationId}", notification.Id);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
