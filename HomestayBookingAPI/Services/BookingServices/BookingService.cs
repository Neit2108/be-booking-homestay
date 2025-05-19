using Hangfire;
using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.Booking;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using HomestayBookingAPI.Services.NotifyServices;
using HomestayBookingAPI.Services.PlaceServices;
using HomestayBookingAPI.Services.UserServices;
using HomestayBookingAPI.Services.VoucherServices;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using System.Security.Claims;

namespace HomestayBookingAPI.Services.BookingServices
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingService> _logger;
        private readonly IPlaceService _placeService;
        private readonly IUserService _userService;
        private readonly IVoucherService _voucherService;
        private readonly INotifyService _notifyService;

        public BookingService(ApplicationDbContext context, ILogger<BookingService> logger, IPlaceService placeService, IUserService userService, IVoucherService voucherService, INotifyService notifyService)
        {
            _context = context;
            _logger = logger;
            _placeService = placeService;
            _userService = userService;
            _voucherService = voucherService;
            _notifyService = notifyService;
        }

        public async Task<double> CalculateTotalPriceAsync(BookingRequest bookingRequest)
        {
            var endDate = bookingRequest.EndDate.Date;
            var startDate = bookingRequest.StartDate.Date;
            int numberOfDays = (endDate - startDate).Days + 1;
            if (startDate == endDate)
            {
                numberOfDays = 1;
            }

            if (numberOfDays <= 0)
            {
                throw new ArgumentException("End date must be after start date");
            }
            //var place = await _placeService.GetPlaceByID(bookingRequest.PlaceId);
            var place = await _context.Places.FindAsync(bookingRequest.PlaceId);


            var pricePerNight = place.Price;

            var totalPrice = pricePerNight * numberOfDays;

            if (bookingRequest.NumberOfGuests >= 3)
            {
                totalPrice += totalPrice * 0.3;
            }
            if ((!string.IsNullOrEmpty(bookingRequest.Voucher)) && (await _voucherService.CheckVoucherAvailable(bookingRequest.Voucher) != null))
            {
                totalPrice = await _voucherService.ApplyVoucherAsync(bookingRequest.Voucher, totalPrice);
                _logger.LogDebug("Voucher được dùng : " + bookingRequest.Voucher);
            }

            return totalPrice;

        }

        public async Task<bool> CheckAvailabilityAsync(int placeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    _logger.LogWarning("Invalid date range: StartDate {StartDate} must be earlier than EndDate {EndDate}", startDate, endDate);
                    throw new ArgumentException("StartDate must be earlier than EndDate");
                }

                var placeExists = await _context.Places.FindAsync(placeId);
                if (placeExists == null)
                {
                    _logger.LogWarning("Place with ID {PlaceId} does not exist", placeId);
                    throw new Exception("Place does not exist");
                }

                var start = startDate.Date;
                var end = endDate.Date;

                var unavailableDays = await _context.PlaceAvailables
                    .Where(pa => pa.PlaceId == placeId &&
                                 pa.Date >= start &&
                                 pa.Date <= end &&
                                 !pa.IsAvailable)
                    .AnyAsync();

                if (unavailableDays)
                {
                    _logger.LogInformation("Place {PlaceId} is not available from {StartDate} to {EndDate}", placeId, startDate, endDate);
                    return false;
                }

                _logger.LogInformation("Place {PlaceId} is available from {StartDate} to {EndDate}", placeId, startDate, endDate);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for place {PlaceId} from {StartDate} to {EndDate}", placeId, startDate, endDate);
                throw new Exception("Error checking availability", ex);
            }
        }

        public async Task<BookingResponse> CreateBookingAsync(BookingRequest bookingRequest)
        {
            if (bookingRequest == null)
            {
                throw new ArgumentNullException(nameof(bookingRequest), "Booking request cannot be null");
            }

            if (string.IsNullOrEmpty(bookingRequest.UserId) || bookingRequest.PlaceId == null)
            {
                throw new ArgumentException("UserId and PlaceId must be provided");
            }

            if (bookingRequest.NumberOfGuests <= 0)
            {
                throw new ArgumentException("Number of guests must be greater than zero");
            }

            if (bookingRequest.StartDate > bookingRequest.EndDate)
            {
                throw new ArgumentException("Start date must be earlier than end date");
            }

            var isAvailable = await CheckAvailabilityAsync(bookingRequest.PlaceId, bookingRequest.StartDate, bookingRequest.EndDate);
            if (!isAvailable)
            {
                throw new InvalidOperationException($"Place {bookingRequest.PlaceId} is not available for the selected dates");
            } //-> Check phòng trống

            var booking = new Booking
            {
                UserId = bookingRequest.UserId,
                PlaceId = bookingRequest.PlaceId,
                StartDate = bookingRequest.StartDate,
                EndDate = bookingRequest.EndDate,
                NumberOfGuests = bookingRequest.NumberOfGuests,
                TotalPrice = await CalculateTotalPriceAsync(bookingRequest),
                Status = bookingRequest.Status,
                PaymentStatus = PaymentStatus.Unpaid,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };


            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var start = bookingRequest.StartDate.Date;
                var end = bookingRequest.EndDate.Date;

                // Các ngày đặt
                var dateRange = Enumerable.Range(0, (end - start).Days + 1)
                    .Select(d => start.AddDays(d))
                    .ToList();

                var placeAvailables = await _context.PlaceAvailables
                    .Where(pa => pa.PlaceId == bookingRequest.PlaceId && dateRange.Contains(pa.Date))
                    .ToDictionaryAsync(pa => pa.Date, pa => pa);

                var updates = new List<PlaceAvailable>();
                foreach (var date in dateRange)
                {
                    if (placeAvailables.TryGetValue(date, out var existing))
                    {
                        existing.IsAvailable = false;
                    }
                    else
                    {
                        updates.Add(new PlaceAvailable
                        {
                            PlaceId = bookingRequest.PlaceId,
                            Date = date,
                            IsAvailable = false
                        });
                    }
                }

                if (updates.Any())
                {
                    _context.PlaceAvailables.AddRange(updates);
                }

                _logger.LogDebug($"Giá tiền : {booking.TotalPrice}");
                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();

                await _notifyService.CreateNewBookingNotificationAsync(booking); //tao thong bao

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to create booking due to an unexpected error", ex);
            }

            return new BookingResponse
            {
                Id = booking.Id,
                UserId = booking.UserId,
                PlaceId = booking.PlaceId,
                PlaceName = booking.Place.Name,
                PlaceAddress = booking.Place.Address,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                NumberOfGuests = booking.NumberOfGuests,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                PaymentStatus = booking.PaymentStatus
            };
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                {
                    return false;
                }
                //if (booking.Status == BookingStatus.Confirmed)
                //{
                //    throw new InvalidOperationException("Cannot delete a confirmed booking");
                //}
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting booking", ex);
            }
        }

        public async Task<IEnumerable<BookingResponse>> GetAllBookingsAsync(
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Bookings.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<BookingStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(b => b.Status == statusEnum);
                }
                else
                {
                    _logger.LogWarning($"Invalid status value: {status}");
                }
            }
            if (startDate.HasValue)
            {
                query = query.Where(b => b.StartDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(b => b.EndDate <= endDate.Value);
            }

            int totalRecords = await query.CountAsync();
            _logger.LogInformation($"Found {totalRecords} bookings before pagination.");

            query = query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var bookings = await query
                .Include(b => b.Place)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    PlaceId = b.PlaceId,
                    PlaceAddress = b.Place.Address,
                    PlaceName = b.Place.Name,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    NumberOfGuests = b.NumberOfGuests,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus,
                    ImageUrl = b.Place.Images != null && b.Place.Images.Any() ? b.Place.Images.FirstOrDefault().ImageUrl : null
                })

                .ToListAsync();

            _logger.LogInformation($"Returning {bookings.Count} bookings after applying filters and pagination.");
            return bookings;
        }

        public async Task<BookingResponse> GetBookingByIdAsync(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Place)
                .Where(b => b.Id == id)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    PlaceId = b.PlaceId,
                    PlaceName = b.Place.Name,
                    PlaceAddress = b.Place.Address,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    NumberOfGuests = b.NumberOfGuests,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus,
                })
                .FirstOrDefaultAsync();
            if (booking == null)
            {
                _logger.LogWarning($"Booking with ID {id} not found");
                return null;
            }
            return booking;
        }

        public async Task<IEnumerable<BookingResponse>> GetBookingsByUserIdAsync(string userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Place)
                    .ThenInclude(p => p.Images)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    PlaceId = b.PlaceId,
                    PlaceName = b.Place.Name,
                    PlaceAddress = b.Place.Address,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    NumberOfGuests = b.NumberOfGuests,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus,
                    ImageUrl = b.Place.Images != null && b.Place.Images.Any()
                        ? b.Place.Images.FirstOrDefault().ImageUrl
                        : null
                })
                .ToListAsync();

            if (!bookings.Any())
            {
                _logger.LogInformation("No bookings found for user {UserId}", userId);
            }

            return bookings;
        }


        public async Task<IEnumerable<BookingResponse>> GetBookingsByLandlordIdAsync(
    string landlordId,
    DateTime? startDate = null,
    DateTime? endDate = null)
        {
            var query = from b in _context.Bookings.AsNoTracking()
                        join p in _context.Places.AsNoTracking().Where(x => x.OwnerId == landlordId)
                            on b.PlaceId equals p.Id
                        select new { Booking = b, Place = p };

            if (startDate.HasValue)
                query = query.Where(x => x.Booking.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.Booking.EndDate <= endDate.Value);

            var list = await query
                .OrderByDescending(x => x.Booking.CreatedAt)
                .GroupJoin(
                    _context.PlaceImages.AsNoTracking(),
                    bp => bp.Place.Id,
                    img => img.PlaceId,
                    (bp, images) => new { bp.Booking, bp.Place, Images = images }
                )
                .SelectMany(
                    x => x.Images.Take(1).DefaultIfEmpty(),
                    (x, img) => new BookingResponse
                    {
                        Id = x.Booking.Id,
                        UserId = x.Booking.UserId,
                        PlaceId = x.Booking.PlaceId,
                        PlaceName = x.Place.Name,
                        PlaceAddress = x.Place.Address,
                        StartDate = x.Booking.StartDate,
                        EndDate = x.Booking.EndDate,
                        NumberOfGuests = x.Booking.NumberOfGuests,
                        TotalPrice = x.Booking.TotalPrice,
                        Status = x.Booking.Status,
                        PaymentStatus = x.Booking.PaymentStatus,
                        ImageUrl = img != null ? img.ImageUrl : null
                    }
                )
                .ToListAsync();

            if (!list.Any())
            {
                _logger.LogInformation("No bookings found for landlord {landlordId}", landlordId);
            }

            return list;
        }

        public async Task<IEnumerable<BookingResponse>> GetBookingsByPlaceIdAsync(int placeId)
        {
            var bookings = await _context.Bookings
                .AsNoTracking()
                .Where(b => b.PlaceId == placeId)
                .Join(
                    _context.Places.Where(p => p.Id == placeId)
                        .Select(p => new { p.Id, p.Name, p.Address }),
                    b => b.PlaceId,
                    p => p.Id,
                    (b, p) => new { Booking = b, Place = p }
                )
                .OrderByDescending(x => x.Booking.CreatedAt)
                .GroupJoin(
                    _context.PlaceImages.Where(pi => pi.PlaceId == placeId).Take(1),
                    bp => bp.Place.Id,
                    pi => pi.PlaceId,
                    (bp, images) => new { bp.Booking, bp.Place, Images = images }
                )
                .SelectMany(
                    x => x.Images.DefaultIfEmpty(),
                    (x, img) => new BookingResponse
                    {
                        Id = x.Booking.Id,
                        UserId = x.Booking.UserId,
                        PlaceId = x.Booking.PlaceId,
                        PlaceName = x.Place.Name,
                        PlaceAddress = x.Place.Address,
                        StartDate = x.Booking.StartDate,
                        EndDate = x.Booking.EndDate,
                        NumberOfGuests = x.Booking.NumberOfGuests,
                        TotalPrice = x.Booking.TotalPrice,
                        Status = x.Booking.Status,
                        PaymentStatus = x.Booking.PaymentStatus,
                        ImageUrl = img != null ? img.ImageUrl : null
                    }
                )

                .ToListAsync();

            if (!bookings.Any())
            {
                _logger.LogInformation("No bookings found for place {PlaceId}", placeId);
            }

            return bookings;
        }

        public async Task<bool> HasConfirmedBookingAsync(string userId, int placeId)
        {
            var hasConfirmedBooking = await _context.Bookings
                .AnyAsync(b => b.UserId == userId && b.PlaceId == placeId && b.Status == BookingStatus.Confirmed);
            return hasConfirmedBooking;
        }

        public async Task<bool> UpdateBookingStatusAsync(int id, BookingStatus status, string currentRole, string rejectReason = "Không xác định")
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tìm booking theo ID
                var booking = await _context.Bookings
                    .Include(b => b.Place) // Include Place để lấy thông tin liên quan nếu cần
                    .ThenInclude(b => b.Owner) // Include Owner để lấy thông tin chủ nhà
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                {
                    _logger.LogWarning("Booking with ID {BookingId} not found for status update", id);
                    return false;
                }

                if (!IsValidStatusTransition(booking.Status, status, currentRole))
                {
                    _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} for booking {BookingId}", booking.Status, status, id);
                    throw new Exception($"Invalid status transition from {booking.Status} to {status}");
                }

                var oldStatus = booking.Status;
                booking.Status = status;
                booking.UpdatedAt = DateTime.UtcNow;


                if (status == BookingStatus.Cancelled && booking.Status != BookingStatus.Cancelled)
                {
                    var start = booking.StartDate.Date;
                    var end = booking.EndDate.Date;

                    var placeAvailables = await _context.PlaceAvailables
                        .Where(pa => pa.PlaceId == booking.PlaceId &&
                                     pa.Date >= start &&
                                     pa.Date <= end)
                        .ToListAsync();

                    if (!placeAvailables.Any())
                    {
                        _logger.LogWarning("No PlaceAvailables found for booking {BookingId} in date range {StartDate} to {EndDate}", id, start, end);
                    }
                    else
                    {
                        foreach (var pa in placeAvailables)
                        {
                            if (!pa.IsAvailable)
                            {
                                pa.IsAvailable = true;
                            }
                        }
                    }
                }

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                if (status == BookingStatus.Confirmed || status == BookingStatus.Cancelled)
                {
                    await _notifyService.NotifyBookingStatusChangeAsync(id, status == BookingStatus.Confirmed, rejectReason);
                }

                await transaction.CommitAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database error occurred while updating status of booking {BookingId}", id);
                throw new Exception("Database error occurred while updating booking status", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error occurred while updating status of booking {BookingId}", id);
                throw new Exception("Unexpected error occurred while updating booking status", ex);
            }
        }

        private bool IsValidStatusTransition(BookingStatus currentStatus, BookingStatus newStatus, string role)
        {
            return (currentStatus, newStatus) switch
            {
                (BookingStatus.Pending, BookingStatus.Confirmed) => true,
                (BookingStatus.Pending, BookingStatus.Cancelled) => true,
                (BookingStatus.Confirmed, BookingStatus.Cancelled) => role == "Admin",
                _ => false
            };
        }

        public async Task<string> ExportBookingsAsync(List<BookingResponse> bookings, string filePath, string exportedBy = "Admin")
        {
            ExcelPackage.License.SetNonCommercialPersonal("HomiesStay");
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Bookings");

                int colCount = 13;
                int dataStartRow = 5;

                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports", "favicon.png");
                if (File.Exists(logoPath))
                {
                    var pic = ws.Drawings.AddPicture("Logo", new FileInfo(logoPath));
                    pic.SetPosition(0, 0, 0, 0);
                    pic.SetSize(90, 90);
                }
                ws.Cells[1, 2, 2, colCount].Merge = true;
                ws.Cells[1, 2].Value = "BÁO CÁO DANH SÁCH ĐẶT PHÒNG HOMESTAY";
                ws.Cells[1, 2].Style.Font.Size = 22;
                ws.Cells[1, 2].Style.Font.Bold = true;
                ws.Cells[1, 2].Style.Font.Color.SetColor(Color.FromArgb(51, 102, 204));
                ws.Cells[1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[1, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(1).Height = 45;

                ws.Cells[2, 2].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}   -   Người xuất: {exportedBy}";
                ws.Cells[2, 2].Style.Font.Size = 12;
                ws.Cells[2, 2].Style.Font.Italic = true;
                ws.Cells[2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                string[] headers = {
            "Mã Đặt Chỗ", "Họ Tên", "CCCD", "Email", "SĐT",
            "Tên Homestay", "Địa Chỉ", "Bắt Đầu", "Kết Thúc",
            "Số Khách", "Tổng Tiền (VNĐ)", "Trạng Thái Đặt", "Thanh Toán"
        };
                Color[] headerColors = {
            Color.FromArgb(66, 133, 244),
            Color.FromArgb(219, 68, 55),
            Color.FromArgb(244, 180, 0),
            Color.FromArgb(15, 157, 88),
            Color.FromArgb(255, 128, 0),
            Color.FromArgb(102, 0, 204),
            Color.FromArgb(0, 153, 255),
            Color.FromArgb(118, 255, 3),
            Color.FromArgb(255, 230, 128),
            Color.FromArgb(255, 102, 178),
            Color.FromArgb(0, 204, 153),
            Color.FromArgb(255, 193, 7),
            Color.FromArgb(76, 175, 80)
        };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[dataStartRow, i + 1].Value = headers[i];
                    ws.Cells[dataStartRow, i + 1].Style.Font.Bold = true;
                    ws.Cells[dataStartRow, i + 1].Style.Font.Size = 12;
                    ws.Cells[dataStartRow, i + 1].Style.Font.Color.SetColor(Color.White);
                    ws.Cells[dataStartRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[dataStartRow, i + 1].Style.Fill.BackgroundColor.SetColor(headerColors[i]);
                    ws.Cells[dataStartRow, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[dataStartRow, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[dataStartRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thick, Color.Black);
                }

                int row = dataStartRow + 1;
                foreach (var b in bookings)
                {
                    var user = await _userService.GetUserByID(b.UserId);
                    if (user == null)
                    {
                        ws.Cells[row, 2].Value = "Không xác định";
                        ws.Cells[row, 3].Value = "N/A";
                        ws.Cells[row, 4].Value = "N/A";
                        ws.Cells[row, 5].Value = "N/A";
                    }
                    else
                    {
                        ws.Cells[row, 2].Value = user.FullName;
                        ws.Cells[row, 3].Value = user.IdentityCard;
                        ws.Cells[row, 4].Value = user.Email;
                        ws.Cells[row, 5].Value = user.PhoneNumber;
                    }

                    ws.Cells[row, 1].Value = b.Id;
                    ws.Cells[row, 6].Value = b.PlaceName;
                    ws.Cells[row, 7].Value = b.PlaceAddress;
                    ws.Cells[row, 8].Value = b.StartDate.ToString("dd/MM/yyyy");
                    ws.Cells[row, 9].Value = b.EndDate.ToString("dd/MM/yyyy");
                    ws.Cells[row, 10].Value = b.NumberOfGuests;
                    ws.Cells[row, 11].Value = b.TotalPrice;

                    ws.Cells[row, 11].Style.Numberformat.Format = "#,##0 [$₫-421]";

                    var statusCell = ws.Cells[row, 12];
                    string status = b.Status.ToString().ToLower() ?? "";
                    if (status.Contains("pending"))
                    {
                        statusCell.Value = "⏳ " + b.Status;
                        statusCell.Style.Font.Color.SetColor(Color.Orange);
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 243, 205));
                    }
                    else if (status.Contains("completed"))
                    {
                        statusCell.Value = "✅ " + b.Status;
                        statusCell.Style.Font.Color.SetColor(Color.SeaGreen);
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(232, 245, 233));
                    }
                    else if (status.Contains("confirmed"))
                    {
                        statusCell.Value = b.Status;
                        statusCell.Style.Font.Color.SetColor(Color.SeaGreen);
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(232, 245, 233));
                    }
                    else if (status.Contains("cancelled"))
                    {
                        statusCell.Value = "❌ " + b.Status;
                        statusCell.Style.Font.Color.SetColor(Color.Red);
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 228, 225));
                    }
                    else
                    {
                        statusCell.Value = b.Status;
                    }
                    statusCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    var paymentCell = ws.Cells[row, 13];
                    string payment = b.PaymentStatus.ToString().ToLower() ?? "";
                    if (payment.Contains("paid"))
                    {
                        paymentCell.Value = "💰 Đã TT";
                        paymentCell.Style.Font.Color.SetColor(Color.SeaGreen);
                        paymentCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        paymentCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 255, 224));
                    }
                    else if (payment.Contains("unpaid"))
                    {
                        paymentCell.Value = "🕓 Chưa TT";
                        paymentCell.Style.Font.Color.SetColor(Color.DarkOrange);
                        paymentCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        paymentCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 249, 196));
                    }
                    else if (payment.Contains("fail"))
                    {
                        paymentCell.Value = "❗Lỗi TT";
                        paymentCell.Style.Font.Color.SetColor(Color.Red);
                        paymentCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        paymentCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 205, 210));
                    }
                    else
                    {
                        paymentCell.Value = b.PaymentStatus;
                    }
                    paymentCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    if (row % 2 == 0)
                    {
                        ws.Cells[row, 1, row, colCount].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, 1, row, colCount].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(243, 246, 249));
                    }


                    ws.Cells[row, 1, row, colCount].Style.Border.BorderAround(ExcelBorderStyle.Dotted, Color.Gray);

                    row++;
                }

                ws.Cells[dataStartRow, 1, row - 1, colCount].AutoFilter = true;
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.View.FreezePanes(dataStartRow + 1, 1);

                ws.Cells[$"A{row + 2}:M{row + 2}"].Merge = true;
                ws.Cells[$"A{row + 2}"].Value = "Ghi chú: File xuất từ hệ thống HomiesStay. Vui lòng không chỉnh sửa dữ liệu trực tiếp trên file này.";
                ws.Cells[$"A{row + 2}"].Style.Font.Italic = true;
                ws.Cells[$"A{row + 2}"].Style.Font.Size = 11;
                ws.Cells[$"A{row + 2}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells[$"A{row + 2}"].Style.Font.Color.SetColor(Color.Gray);

                ws.Cells[$"M{row + 5}"].Value = "HomiesStay.vn";
                ws.Cells[$"M{row + 5}"].Style.Font.Size = 18;
                ws.Cells[$"M{row + 5}"].Style.Font.Color.SetColor(Color.FromArgb(232, 234, 246));
                ws.Cells[$"M{row + 5}"].Style.Font.Italic = true;

                await package.SaveAsAsync(new FileInfo(filePath));
            }
            return filePath;
        }
    }
}
