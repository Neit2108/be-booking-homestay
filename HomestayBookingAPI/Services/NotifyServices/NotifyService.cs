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

        public async Task CreatePaymentFailureNotificationAsync(Booking booking)
        {
            var customer = await _context.Users.FindAsync(booking.UserId);
            var place = await _context.Places.FirstOrDefaultAsync(p => p.Id == booking.PlaceId);

            if (customer == null || place == null)
            {
                throw new Exception("Customer or Place not found");
            }

            // Tạo token cho khách hàng
            var customerToken = _jwtService.GenerateActionToken(customer.Id, NotificationType.PaymentFailure.ToString(), booking.Id, "Customer");

            // Tạo thông báo cho khách hàng
            var customerNotify = new Notification
            {
                RecipientId = customer.Id,
                SenderId = "system",
                BookingId = booking.Id,
                Type = NotificationType.PaymentFailure,
                Title = "Thanh toán không thành công",
                Message = $"Thanh toán đặt phòng {place.Name} từ {booking.StartDate.ToShortDateString()} đến {booking.EndDate.ToShortDateString()} không thành công. Vui lòng thử lại.",
                Url = $"{_baseUrl}/booking/payment/{booking.Id}",
                Status = NotificationStatus.Pending,
            };

            try
            {
                // Tạo email cho khách hàng
                var customerEmail = TemplateMail.PaymentFailureEmail(booking, customerNotify.Url);

                // Gửi email qua Hangfire
                var customerJobId = _backgroundJobClient.Enqueue(() => _emailService.SendEmailAsync(customer.Email, "Thanh toán không thành công", customerEmail));
                customerNotify.JobId = customerJobId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment failure email");
                customerNotify.Status = NotificationStatus.Failed;
            }

            // Lưu thông báo vào database
            _context.Notifications.Add(customerNotify);
            await _context.SaveChangesAsync();
        }

        public async Task CreatePaymentSuccessNotificationAsync(Booking booking)
        {
            // Lấy thông tin người dùng và homestay
            var customer = await _context.Users.FindAsync(booking.UserId);
            var place = await _context.Places.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == booking.PlaceId);
            var landlord = place?.Owner;

            if (customer == null || place == null || landlord == null)
            {
                throw new Exception("Customer, Place or Landlord not found");
            }

            // Tạo token cho khách hàng
            var customerToken = _jwtService.GenerateActionToken(
                customer.Id,
                NotificationType.PaymentSuccess.ToString(),
                booking.Id,
                "Tenant"
            );

            // Tạo thông báo cho khách hàng
            var customerNotify = new Notification
            {
                RecipientId = customer.Id,
                SenderId = "system",
                BookingId = booking.Id,
                Type = NotificationType.PaymentSuccess,
                Title = "Thanh toán thành công",
                Message = $"Thanh toán đặt phòng tại {place.Name} từ {booking.StartDate.ToShortDateString()} đến {booking.EndDate.ToShortDateString()} đã thành công. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.",
                Url = $"{_baseUrl}/auth/verify-action/{customerToken}",
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            // Tạo token cho chủ nhà
            var landlordToken = _jwtService.GenerateActionToken(
                landlord.Id,
                NotificationType.PaymentSuccess.ToString(),
                booking.Id,
                "Landlord"
            );

            // Tạo thông báo cho chủ nhà
            var landlordNotify = new Notification
            {
                RecipientId = landlord.Id,
                SenderId = "system",
                BookingId = booking.Id,
                Type = NotificationType.PaymentSuccess,
                Title = "Thanh toán đặt phòng thành công",
                Message = $"Khách hàng {customer.FullName} đã thanh toán thành công cho đặt phòng tại {place.Name} từ {booking.StartDate.ToShortDateString()} đến {booking.EndDate.ToShortDateString()}.",
                Url = $"{_baseUrl}/auth/verify-action/{landlordToken}",
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            try
            {
                // Tạo email cho khách hàng
                _logger.LogDebug("Creating email for customer {CustomerId} for booking {BookingId}", customer.Id, booking.Id);
                _logger.LogDebug("Url: {Url}", customerNotify.Url);
                var customerEmailTemplate = TemplateMail.PaymentSuccessEmail(booking, customerNotify.Url);

                // Tạo email cho chủ nhà
                _logger.LogInformation("Creating email for landlord {LandlordId} for booking {BookingId}", landlord.Id, booking.Id);
                _logger.LogInformation("Url: {Url}", landlordNotify.Url);
                var landlordEmailTemplate = TemplateMail.LandlordPaymentNotificationEmail(booking, landlordNotify.Url);

                // Gửi email qua Hangfire
                var customerJobId = _backgroundJobClient.Enqueue(() =>
                    _emailService.SendEmailAsync(
                        customer.Email,
                        "Xác nhận thanh toán thành công",
                        customerEmailTemplate
                    )
                );

                var landlordJobId = _backgroundJobClient.Enqueue(() =>
                    _emailService.SendEmailAsync(
                        landlord.Email,
                        "Thông báo thanh toán từ khách hàng",
                        landlordEmailTemplate
                    )
                );

                // Cập nhật jobId trong notification
                customerNotify.JobId = customerJobId;
                landlordNotify.JobId = landlordJobId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment success notification emails for booking {BookingId}", booking.Id);

                // Đánh dấu thông báo là thất bại nếu không gửi được email
                customerNotify.Status = NotificationStatus.Failed;
                landlordNotify.Status = NotificationStatus.Failed;

            }

            // Cập nhật trạng thái booking
            booking.PaymentStatus = PaymentStatus.Paid;
            booking.UpdatedAt = DateTime.UtcNow;
            _context.Bookings.Update(booking);

            await _context.Notifications.AddRangeAsync(customerNotify, landlordNotify);

            await _context.SaveChangesAsync();

            _logger.LogDebug("Payment success notification created for booking {BookingId}", booking.Id);
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
            await _context.Notifications.AddAsync(customerNotify);
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
