using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Payment;
using HomestayBookingAPI.Models.Enum;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.BookingServices;
using HomestayBookingAPI.Services.NotifyServices;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using HomestayBookingAPI.Utils;
using QRCoder;
using HomestayBookingAPI.Services.WalletServices;

namespace HomestayBookingAPI.Services.PaymentServices
{
    public class VNPayService : IVNPayService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOptions<VNPayConfig> _vnpayConfig;
        private readonly ILogger<VNPayService> _logger;
        private readonly INotifyService _notifyService;
        private readonly IBookingService _bookingService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWalletService _walletService;

        public VNPayService(
            ApplicationDbContext context,
            IOptions<VNPayConfig> vnpayConfig,
            ILogger<VNPayService> logger,
            INotifyService notifyService,
            IBookingService bookingService,
            IHttpClientFactory httpClientFactory,
            IWalletService walletService)
        {
            _context = context;
            _vnpayConfig = vnpayConfig;
            _logger = logger;
            _notifyService = notifyService;
            _bookingService = bookingService;
            _httpClientFactory = httpClientFactory;
            _walletService = walletService;
        }

        private string GenerateQRCodeBase64(string url)
        {
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

                // Sử dụng PngByteQRCode thay vì QRCode (phụ thuộc System.Drawing)
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeBytes = qrCode.GetGraphic(20); // Kích thước 20 pixel mỗi module

                return Convert.ToBase64String(qrCodeBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                return null;
            }
        }

        public async Task<VNPayCreateResponse> CreatePaymentUrlAsync(VNPayCreateRequest request, string ipAddress)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId);

            if (booking == null)
            {
                throw new Exception($"Booking with ID {request.BookingId} not found");
            }

            // Kiểm tra xem booking đã được thanh toán chưa
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == request.BookingId && p.Status == "Success");

            if (existingPayment != null)
            {
                throw new Exception("This booking has already been paid");
            }

            // Tạo mới payment record
            var payment = new Payment
            {
                BookingId = booking.Id,
                UserId = booking.UserId,
                Amount = booking.TotalPrice,
                PaymentMethod = "VNPAY",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                TransactionId = "PENDING_" + Guid.NewGuid().ToString("N").Substring(0, 10),
                PaymentUrl = "pending", // Giá trị tạm thời
                QrCodeUrl = "pending"   // Giá trị tạm thời
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var createDate = vietnamNow.ToString("yyyyMMddHHmmss");
            var expireDate = vietnamNow.AddMinutes(15).ToString("yyyyMMddHHmmss");

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _vnpayConfig.Value.TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(booking.TotalPrice * 100)).ToString()); // Nhân 100 vì VNPay tính tiền theo VND * 100
            vnpay.AddRequestData("vnp_CreateDate", createDate);
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", request.Locale ?? "vn");
            vnpay.AddRequestData("vnp_OrderInfo", request.OrderInfo ?? $"Thanh toán đặt phòng #{booking.Id}");
            vnpay.AddRequestData("vnp_OrderType", request.OrderType ?? "270001"); // Mã danh mục hàng hóa
            vnpay.AddRequestData("vnp_ReturnUrl", !string.IsNullOrEmpty(request.ReturnUrl) ? request.ReturnUrl : _vnpayConfig.Value.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", payment.Id.ToString()); // Sử dụng payment ID làm mã tham chiếu
            vnpay.AddRequestData("vnp_ExpireDate", expireDate);

            // Thêm BankCode param nếu có
            if (!string.IsNullOrEmpty(request.BankCode))
            {
                vnpay.AddRequestData("vnp_BankCode", request.BankCode);
            }

            // Tạo URL thanh toán
            string paymentUrl = vnpay.CreateRequestUrl(_vnpayConfig.Value.PaymentUrl, _vnpayConfig.Value.HashSecret);

            // Tạo URL QR trực tiếp - Sử dụng VNPay API nếu là thanh toán ngân hàng
            string qrCodeUrl = paymentUrl;
            string qrCodeBase64 = null;

            // Nếu là chọn hình thức chuyển khoản, tạo QR code trực tiếp
            if (request.BankCode == "VNPAYQR" || string.IsNullOrEmpty(request.BankCode))
            {
                try
                {
                    // Tạo URL trực tiếp đến QR code của VNPay (nếu có API)
                    // Hoặc sử dụng thư viện QRCode để tạo QR từ URL
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(paymentUrl, QRCodeGenerator.ECCLevel.Q);

                    // Sử dụng PngByteQRCode 
                    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                    byte[] qrCodeBytes = qrCode.GetGraphic(20); // Kích thước 20 pixel mỗi module
                    qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);

                    // Hoặc thử sử dụng VNPay API trực tiếp để lấy QR URL nếu họ có hỗ trợ
                    // qrCodeUrl = await GetQRDirectLinkAsync(paymentUrl, booking.TotalPrice, payment.Id.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating QR code");
                    // Sử dụng URL thanh toán làm QR URL nếu tạo QR thất bại
                    qrCodeUrl = paymentUrl;
                }
            }

            // Cập nhật URL vào payment record
            payment.PaymentUrl = paymentUrl;
            payment.QrCodeUrl = qrCodeUrl;
            await _context.SaveChangesAsync();

            return new VNPayCreateResponse
            {
                PaymentId = payment.Id,
                PaymentUrl = paymentUrl,
                QrCodeUrl = qrCodeUrl,
                QrCodeBase64 = qrCodeBase64,  // Trả về QR code base64 nếu có
                ExpireDate = DateTime.Now.AddMinutes(15)
            };
        }

        private async Task<string> GetQRDirectLinkAsync(string paymentUrl, double amount, string txnRef)
        {
            try
            {
                // Đây chỉ là mẫu, bạn cần thay thế bằng API thực tế của VNPay nếu có
                var client = _httpClientFactory.CreateClient();

                var response = await client.GetAsync($"https://merchant.vnpay.vn/qrcode?amount={amount}&txnRef={txnRef}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    return content; // Thay bằng việc parse JSON nếu cần
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting QR direct link from VNPay");
            }

            // Trả về URL thanh toán gốc nếu không lấy được QR trực tiếp
            return paymentUrl;
        }

        public async Task<PaymentResponse> ProcessPaymentCallbackAsync(Dictionary<string, string> vnpayData)
        {
            _logger.LogInformation("VNPay Callback Processing Started");
            _logger.LogInformation("Received VNPay Data: {@vnpayData}", vnpayData);

            // Xác thực dữ liệu callback từ VNPay
            if (!vnpayData.TryGetValue("vnp_TxnRef", out var txnRef) ||
                !vnpayData.TryGetValue("vnp_ResponseCode", out var responseCode) ||
                !vnpayData.TryGetValue("vnp_TransactionStatus", out var transactionStatus) ||
                !vnpayData.TryGetValue("vnp_SecureHash", out var secureHash))
            {
                throw new Exception("Invalid VNPay callback data");
            }

            // Validate transaction reference
            if (!int.TryParse(txnRef, out int paymentId))
            {
                throw new Exception("Invalid payment ID");
            }

            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                throw new Exception($"Payment with ID {paymentId} not found");
            }

            // Validate secure hash
            var vnpay = new VnPayLibrary();

            foreach (var kvp in vnpayData.Where(x => x.Key != "vnp_SecureHash" && x.Key != "vnp_SecureHashType"))
            {
                vnpay.AddResponseData(kvp.Key, kvp.Value);
            }

            // Verify secure hash
            string inputHash = vnpayData["vnp_SecureHash"];
            bool isValidSignature = vnpay.ValidateSignature(inputHash, _vnpayConfig.Value.HashSecret);

            if (!isValidSignature)
            {
                _logger.LogWarning("Invalid VNPay secure hash");
                throw new Exception("Invalid signature from VNPay");
            }
            _logger.LogInformation($"Transaction Reference: {txnRef}");
            _logger.LogInformation($"Response Code: {responseCode}");
            _logger.LogInformation($"Transaction Status: {transactionStatus}");
            _logger.LogInformation($"Secure Hash Validation: {isValidSignature}");

            // Process payment result
            bool isSuccess = (responseCode == "00" || responseCode == "0") &&
                     (transactionStatus == "00" || transactionStatus == "0");

            _logger.LogInformation($"Payment Considered Successful: {isSuccess}");
            string paymentStatus = isSuccess ? "Success" : "Failed";

            // Update payment record
            payment.Status = paymentStatus;
            payment.TransactionId = vnpayData.ContainsKey("vnp_TransactionNo") ? vnpayData["vnp_TransactionNo"] : null;

            if (isSuccess)
            {
                payment.PaymentDate = DateTime.UtcNow;

                // Xử lý theo mục đích thanh toán
                if (payment.Purpose == PaymentPurpose.BookingPayment && payment.BookingId != null)
                {
                    // Cập nhật trạng thái booking
                    payment.Booking.PaymentStatus = PaymentStatus.Paid;

                    // Gửi thông báo thanh toán thành công
                    await _notifyService.CreatePaymentSuccessNotificationAsync(payment.Booking);
                }
                else if (payment.Purpose == PaymentPurpose.WalletDeposit)
                {
                    // Tìm ví của người dùng và nạp tiền
                    var walletTransaction = await _walletService.AddTransactionAsync(
                        payment.UserId,
                        payment.Amount,
                        TransactionType.Deposit,
                        $"Nạp tiền qua VNPAY - Mã GD: {payment.TransactionId}",
                        null,
                        payment.Id
                    );
                    walletTransaction.Payment = payment;
                    
                    var wallet = await _context.Wallets
                        .Include(w => w.User)
                        .FirstOrDefaultAsync(w => w.UserId == payment.UserId);
                    if (wallet != null)
                    {
                        walletTransaction.Wallet = wallet;
                    }

                    // Gửi thông báo nạp tiền thành công
                    await _notifyService.CreateWalletTransactionNotificationAsync(walletTransaction);

                }
            }


            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                Id = payment.Id,
                BookingId = payment.BookingId ?? 0,
                UserId = payment.UserId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                PaymentUrl = payment.PaymentUrl,
                QrCodeUrl = payment.QrCodeUrl,
                CreatedAt = payment.CreatedAt,
                PaymentDate = payment.PaymentDate
            };
        }

        public async Task<PaymentResponse> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return null;
            }

            return new PaymentResponse
            {
                Id = payment.Id,
                BookingId = payment.BookingId ?? 0,
                UserId = payment.UserId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                PaymentUrl = payment.PaymentUrl,
                QrCodeUrl = payment.QrCodeUrl,
                CreatedAt = payment.CreatedAt,
                PaymentDate = payment.PaymentDate
            };
        }

        public async Task<IEnumerable<PaymentResponse>> GetPaymentsByBookingIdAsync(int bookingId)
        {
            var payments = await _context.Payments
                .Where(p => p.BookingId == bookingId)
                .ToListAsync();

            return payments.Select(p => new PaymentResponse
            {
                Id = p.Id,
                BookingId = p.BookingId ?? 0,
                UserId = p.UserId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                TransactionId = p.TransactionId,
                PaymentUrl = p.PaymentUrl,
                QrCodeUrl = p.QrCodeUrl,
                CreatedAt = p.CreatedAt,
                PaymentDate = p.PaymentDate
            });
        }

        public async Task<IEnumerable<PaymentResponse>> GetPaymentsByUserIdAsync(string userId)
        {
            var payments = await _context.Payments
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return payments.Select(p => new PaymentResponse
            {
                Id = p.Id,
                BookingId = p.BookingId ?? 0,
                UserId = p.UserId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                TransactionId = p.TransactionId,
                PaymentUrl = p.PaymentUrl,
                QrCodeUrl = p.QrCodeUrl,
                CreatedAt = p.CreatedAt,
                PaymentDate = p.PaymentDate
            });
        }

        public async Task<VNPayCreateResponse> CreateGenericPaymentAsync(GenericPaymentRequest request, string userId, string ipAddress)
        {
            // Kiểm tra dữ liệu đầu vào
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Số tiền phải lớn hơn 0");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception($"Không tìm thấy người dùng với ID {userId}");
            }

            // Xác định OrderInfo nếu chưa có
            if (string.IsNullOrEmpty(request.OrderInfo))
            {
                request.OrderInfo = request.Purpose == PaymentPurpose.WalletDeposit
                    ? $"Nạp {request.Amount:N0} VND vào ví"
                    : $"Thanh toán đặt phòng #{request.BookingId}";
            }

            // Xác định OrderType nếu chưa có
            if (string.IsNullOrEmpty(request.OrderType))
            {
                request.OrderType = request.Purpose == PaymentPurpose.WalletDeposit
                    ? "200000" // Mã loại hình nạp ví
                    : "270001"; // Mã loại hình mặc định
            }

            // Tạo Payment record
            var payment = new Payment
            {
                UserId = userId,
                BookingId = (request.Purpose == PaymentPurpose.BookingPayment ? request.BookingId : null),
                Amount = request.Amount,
                PaymentMethod = "VNPAY",
                Status = "Pending",
                Purpose = request.Purpose,
                CreatedAt = DateTime.UtcNow,
                TransactionId = "PENDING_" + Guid.NewGuid().ToString("N").Substring(0, 10),
                PaymentUrl = "pending",
                QrCodeUrl = "pending"
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            // Tạo URL thanh toán VNPay
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var createDate = vietnamNow.ToString("yyyyMMddHHmmss");
            var expireDate = vietnamNow.AddMinutes(15).ToString("yyyyMMddHHmmss");

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _vnpayConfig.Value.TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", createDate);
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", request.Locale);
            vnpay.AddRequestData("vnp_OrderInfo", request.OrderInfo);
            vnpay.AddRequestData("vnp_OrderType", request.OrderType);
            vnpay.AddRequestData("vnp_ReturnUrl", request.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", payment.Id.ToString());
            vnpay.AddRequestData("vnp_ExpireDate", expireDate);

            if (!string.IsNullOrEmpty(request.BankCode))
            {
                vnpay.AddRequestData("vnp_BankCode", request.BankCode);
            }

            // Tạo URL thanh toán
            string paymentUrl = vnpay.CreateRequestUrl(_vnpayConfig.Value.PaymentUrl, _vnpayConfig.Value.HashSecret);

            // Tạo QR code
            string qrCodeUrl = paymentUrl;
            string qrCodeBase64 = null;

            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(paymentUrl, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeBytes = qrCode.GetGraphic(20);
                qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo QR code");
            }

            // Cập nhật URL vào payment record
            payment.PaymentUrl = paymentUrl;
            payment.QrCodeUrl = qrCodeUrl;
            await _context.SaveChangesAsync();

            return new VNPayCreateResponse
            {
                PaymentId = payment.Id,
                PaymentUrl = paymentUrl,
                QrCodeUrl = qrCodeUrl,
                QrCodeBase64 = qrCodeBase64,
                ExpireDate = DateTime.Now.AddMinutes(15)
            };
        }
    }
}
