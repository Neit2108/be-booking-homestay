using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.Booking;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using HomestayBookingAPI.Services.BookingServices;
using HomestayBookingAPI.Services.NotifyServices;
using HomestayBookingAPI.Services.PlaceServices;
using HomestayBookingAPI.Services.UserServices;
using HomestayBookingAPI.Services.VoucherServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HomestayBookingAPI.Tests.Services
{
    public class BookingServiceTests
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<ILogger<BookingService>> _mockLogger;
        private readonly Mock<IPlaceService> _mockPlaceService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IVoucherService> _mockVoucherService;
        private readonly Mock<INotifyService> _mockNotifyService;
        private readonly BookingService _bookingService;
        private readonly List<Booking> _bookings;
        private readonly List<PlaceAvailable> _placeAvailables;

        public BookingServiceTests()
        {
            // Sample data
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser",
                Email = "test@example.com",
                FullName = "Test User"
            };

            var place = new Place
            {
                Id = 1,
                Name = "Test Place",
                Price = 100.0,
                OwnerId = "owner1",
                Owner = new ApplicationUser { Id = "owner1", Email = "owner@example.com" },
                Images = new List<PlaceImage>
                {
                    new PlaceImage { Id = 1, PlaceId = 1, ImageUrl = "https://example.com/image.jpg" }
                },
                MaxGuests = 4
            };

            _bookings = new List<Booking>
            {
                new Booking
                {
                    Id = 1,
                    UserId = "user1",
                    User = user,
                    PlaceId = 1,
                    Place = place,
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(3),
                    NumberOfGuests = 2,
                    TotalPrice = 200.0,
                    Status = BookingStatus.Pending,
                    PaymentStatus = PaymentStatus.Unpaid,
                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                    UpdatedAt = DateTime.UtcNow.AddHours(-1)
                },
                new Booking
                {
                    Id = 2,
                    UserId = "user1",
                    User = user,
                    PlaceId = 2,
                    StartDate = DateTime.UtcNow.AddDays(5),
                    EndDate = DateTime.UtcNow.AddDays(7),
                    NumberOfGuests = 3,
                    TotalPrice = 300.0,
                    Status = BookingStatus.Confirmed,
                    PaymentStatus = PaymentStatus.Paid,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _placeAvailables = new List<PlaceAvailable>
            {
                new PlaceAvailable
                {
                    Id = 1,
                    PlaceId = 1,
                    Date = DateTime.UtcNow.AddDays(1).Date,
                    IsAvailable = true
                },
                new PlaceAvailable
                {
                    Id = 2,
                    PlaceId = 1,
                    Date = DateTime.UtcNow.AddDays(2).Date,
                    IsAvailable = true
                },
                new PlaceAvailable
                {
                    Id = 3,
                    PlaceId = 1,
                    Date = DateTime.UtcNow.AddDays(3).Date,
                    IsAvailable = true
                }
            };

            // Setup mocks
            _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            _mockContext.Setup(c => c.Bookings).ReturnsDbSet(_bookings);
            _mockContext.Setup(c => c.PlaceAvailables).ReturnsDbSet(_placeAvailables);

            _mockLogger = new Mock<ILogger<BookingService>>();
            _mockPlaceService = new Mock<IPlaceService>();
            _mockUserService = new Mock<IUserService>();
            _mockVoucherService = new Mock<IVoucherService>();
            _mockNotifyService = new Mock<INotifyService>();

            // Setup PlaceService to return a place 
            _mockPlaceService.Setup(s => s.GetPlaceByID(1))
                .ReturnsAsync(new PlaceDTO
                {
                    Id = 1,
                    Name = "Test Place",
                    Price = 100.0,
                    MaxGuests = 4,
                    Images = new List<PlaceImageDTO>
                    {
                        new PlaceImageDTO { Id = 1, PlaceId = 1, ImageUrl = "https://example.com/image.jpg" }
                    }
                });

            // Create service
            _bookingService = new BookingService(
                _mockContext.Object,
                _mockLogger.Object,
                _mockPlaceService.Object,
                _mockUserService.Object,
                _mockVoucherService.Object,
                _mockNotifyService.Object);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithValidId_ReturnsBookingResponse()
        {
            // Arrange
            int bookingId = 1;

            // Act
            var result = await _bookingService.GetBookingByIdAsync(bookingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookingId, result.Id);
            Assert.Equal("user1", result.UserId);
            Assert.Equal(1, result.PlaceId);
            Assert.Equal(2, result.NumberOfGuests);
            Assert.Equal(200.0, result.TotalPrice);
            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            int nonExistentBookingId = 999;

            // Act
            var result = await _bookingService.GetBookingByIdAsync(nonExistentBookingId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetBookingsByUserIdAsync_WithValidUserId_ReturnsBookings()
        {
            // Arrange
            string userId = "user1";

            // Act
            var result = await _bookingService.GetBookingsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, b => b.Id == 1);
            Assert.Contains(result, b => b.Id == 2);
            Assert.All(result, b => Assert.Equal(userId, b.UserId));
        }

        [Fact]
        public async Task GetBookingsByUserIdAsync_WithNonExistentUserId_ReturnsEmptyList()
        {
            // Arrange
            string nonExistentUserId = "nonexistent";

            // Act
            var result = await _bookingService.GetBookingsByUserIdAsync(nonExistentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBookingsByPlaceIdAsync_WithValidPlaceId_ReturnsBookings()
        {
            // Arrange
            int placeId = 1;

            // Setup PlaceService to return a place with images
            _mockPlaceService.Setup(s => s.GetPlaceByID(placeId))
                .ReturnsAsync(new PlaceDTO
                {
                    Id = placeId,
                    Name = "Test Place",
                    Images = new List<PlaceImageDTO>
                    {
                        new PlaceImageDTO { Id = 1, PlaceId = placeId, ImageUrl = "https://example.com/image.jpg" }
                    }
                });

            // Act
            var result = await _bookingService.GetBookingsByPlaceIdAsync(placeId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(placeId, result.First().PlaceId);
            Assert.Contains(result, b => b.Id == 1);
        }

        [Fact]
        public async Task CalculateTotalPriceAsync_WithNormalBooking_ReturnsCorrectPrice()
        {
            // Arrange
            var bookingRequest = new BookingRequest
            {
                PlaceId = 1,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(2),
                NumberOfGuests = 2
            };

            // 3 days at $100/day = $300
            double expectedPrice = 300.0;

            // Act
            var result = await _bookingService.CalculateTotalPriceAsync(bookingRequest);

            // Assert
            Assert.Equal(expectedPrice, result);
        }

        [Fact]
        public async Task CalculateTotalPriceAsync_WithLargeGroup_AppliesSurcharge()
        {
            // Arrange
            var bookingRequest = new BookingRequest
            {
                PlaceId = 1,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(2),
                NumberOfGuests = 3 // 3 or more guests triggers the 30% surcharge
            };

            // 3 days at $100/day = $300
            // With 30% surcharge = $390
            double expectedPrice = 390.0;

            // Act
            var result = await _bookingService.CalculateTotalPriceAsync(bookingRequest);

            // Assert
            Assert.Equal(expectedPrice, result);
        }

        [Fact]
        public async Task CalculateTotalPriceAsync_WithVoucher_AppliesDiscount()
        {
            // Arrange
            var bookingRequest = new BookingRequest
            {
                PlaceId = 1,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(2),
                NumberOfGuests = 2,
                Voucher = "VALID10" // 10% discount
            };

            // Setup VoucherService to check and apply voucher
            _mockVoucherService.Setup(s => s.CheckVoucherAvailable("VALID10"))
                .ReturnsAsync(new DTOs.Voucher.VoucherResponse { Code = "VALID10", Discount = 10 });

            _mockVoucherService.Setup(s => s.ApplyVoucherAsync("VALID10", 300.0))
                .ReturnsAsync(270.0); // 10% off 300 = 270

            // Act
            var result = await _bookingService.CalculateTotalPriceAsync(bookingRequest);

            // Assert
            Assert.Equal(270.0, result);
        }

        [Fact]
        public async Task CheckAvailabilityAsync_WithAvailableDates_ReturnsTrue()
        {
            // Arrange
            int placeId = 1;
            var startDate = DateTime.UtcNow.AddDays(1).Date;
            var endDate = DateTime.UtcNow.AddDays(3).Date;

            // Act
            var result = await _bookingService.CheckAvailabilityAsync(placeId, startDate, endDate);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckAvailabilityAsync_WithUnavailableDates_ReturnsFalse()
        {
            // Arrange
            int placeId = 1;
            var startDate = DateTime.UtcNow.AddDays(1).Date;
            var endDate = DateTime.UtcNow.AddDays(3).Date;

            // Make one of the dates unavailable
            _placeAvailables.First(pa => pa.Date == startDate.AddDays(1)).IsAvailable = false;

            // Act
            var result = await _bookingService.CheckAvailabilityAsync(placeId, startDate, endDate);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckAvailabilityAsync_WithInvalidDateRange_ThrowsException()
        {
            // Arrange
            int placeId = 1;
            var startDate = DateTime.UtcNow.AddDays(3).Date; // End date before start date
            var endDate = DateTime.UtcNow.AddDays(1).Date;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _bookingService.CheckAvailabilityAsync(placeId, startDate, endDate));
        }

        [Fact]
        public async Task CreateBookingAsync_WithValidRequest_CreatesBooking()
        {
            // Arrange
            var bookingRequest = new BookingRequest
            {
                UserId = "user1",
                PlaceId = 1,
                StartDate = DateTime.UtcNow.AddDays(1).Date,
                EndDate = DateTime.UtcNow.AddDays(3).Date,
                NumberOfGuests = 2,
                Status = BookingStatus.Pending
            };

            // Setup for database transaction
            var dbContextTransaction = new Mock<IDbContextTransaction>();
            _mockContext.Setup(m => m.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextTransaction.Object);

            // Capture the booking that's added to the context
            Booking capturedBooking = null;
            _mockContext.Setup(c => c.Bookings.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, ct) => capturedBooking = b)
                .ReturnsAsync((Booking b, CancellationToken ct) => null);

            // Setup PlaceAvailables behavior
            var dateRange = new List<DateTime>
            {
                DateTime.UtcNow.AddDays(1).Date,
                DateTime.UtcNow.AddDays(2).Date,
                DateTime.UtcNow.AddDays(3).Date
            };

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Setup NotifyService
            _mockNotifyService.Setup(n => n.CreateNewBookingNotificationAsync(It.IsAny<Booking>(), true))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bookingService.CreateBookingAsync(bookingRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookingRequest.UserId, result.UserId);
            Assert.Equal(bookingRequest.PlaceId, result.PlaceId);
            Assert.Equal(bookingRequest.StartDate, result.StartDate);
            Assert.Equal(bookingRequest.EndDate, result.EndDate);
            Assert.Equal(bookingRequest.NumberOfGuests, result.NumberOfGuests);
            Assert.Equal(bookingRequest.Status, result.Status);

            // Verify the booking was saved correctly
            Assert.NotNull(capturedBooking);
            Assert.Equal(bookingRequest.UserId, capturedBooking.UserId);
            Assert.Equal(bookingRequest.PlaceId, capturedBooking.PlaceId);
            Assert.Equal(bookingRequest.StartDate, capturedBooking.StartDate);
            Assert.Equal(bookingRequest.EndDate, capturedBooking.EndDate);
            Assert.Equal(bookingRequest.NumberOfGuests, capturedBooking.NumberOfGuests);
            Assert.Equal(PaymentStatus.Unpaid, capturedBooking.PaymentStatus);

            // Verify notifications were sent
            _mockNotifyService.Verify(n => n.CreateNewBookingNotificationAsync(It.IsAny<Booking>(), true), Times.Once);

            // Verify transaction was committed
            dbContextTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_WithUnavailableDates_ThrowsException()
        {
            // Arrange
            var bookingRequest = new BookingRequest
            {
                UserId = "user1",
                PlaceId = 1,
                StartDate = DateTime.UtcNow.AddDays(1).Date,
                EndDate = DateTime.UtcNow.AddDays(3).Date,
                NumberOfGuests = 2,
                Status = BookingStatus.Pending
            };

            // Make one of the dates unavailable
            _placeAvailables.First(pa => pa.Date == bookingRequest.StartDate.AddDays(1)).IsAvailable = false;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _bookingService.CreateBookingAsync(bookingRequest));
        }

        [Fact]
        public async Task UpdateBookingStatusAsync_FromPendingToConfirmed_UpdatesStatusAndNotifies()
        {
            // Arrange
            int bookingId = 1;
            BookingStatus newStatus = BookingStatus.Confirmed;
            string currentRole = "Landlord";

            // Setup booking retrieval with Include
            _mockContext.Setup(c => c.Bookings
                .Include("Place.Owner"))
                .Returns(_bookings.AsQueryable());

            // Setup ThenInclude continuation
            var booking = _bookings.First(b => b.Id == bookingId);

            //_mockContext.Setup(c => c.Bookings
            //    .Include("Place.Owner"))
            //    .ThenInclude(It.IsAny<Func<object, object>>()))
            //    .Returns(_bookings.AsQueryable());
            _mockContext.Setup(c => c.Bookings
                .Include("Place.Owner"))
                .Returns(_bookings.AsQueryable());

            // Set up FirstOrDefaultAsync
            _mockContext.Setup(c => c.Bookings
                .Include("Place.Owner")
                .FirstOrDefaultAsync(It.IsAny<Expression<Func<Booking, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expression<Func<Booking, bool>> predicate, CancellationToken token) =>
                    _bookings.AsQueryable().FirstOrDefault(predicate.Compile()));

            // Setup NotifyService
            _mockNotifyService.Setup(n => n.NotifyBookingStatusChangeAsync(bookingId, true, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Setup SaveChangesAsync
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Setup for database transaction
            var dbContextTransaction = new Mock<IDbContextTransaction>();
            _mockContext.Setup(m => m.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextTransaction.Object);

            // Act
            var result = await _bookingService.UpdateBookingStatusAsync(bookingId, newStatus, currentRole);

            // Assert
            Assert.True(result);
            Assert.Equal(newStatus, booking.Status);

            // Verify notification was sent
            _mockNotifyService.Verify(n => n.NotifyBookingStatusChangeAsync(bookingId, true, It.IsAny<string>()), Times.Once);

            // Verify transaction was committed
            dbContextTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBookingStatusAsync_FromPendingToCancelled_UpdatesStatusNotifiesAndFreesAvailability()
        {
            // Arrange
            int bookingId = 1;
            BookingStatus newStatus = BookingStatus.Cancelled;
            string currentRole = "Landlord";
            string rejectReason = "Place unavailable";

            // Setup booking retrieval with Include
            _mockContext.Setup(c => c.Bookings
                .Include("Place.Owner"))
                .Returns(_bookings.AsQueryable());

            // Setup ThenInclude continuation
            var booking = _bookings.First(b => b.Id == bookingId);

            _mockContext.Setup(c => c.Bookings
                .Include("Place.Owner"))
                .Returns(_bookings.AsQueryable());

            // Set up FirstOrDefaultAsync
            _mockContext.Setup(c => c.Bookings
                .Include("Place.Owner")
                .FirstOrDefaultAsync(It.IsAny<Expression<Func<Booking, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expression<Func<Booking, bool>> predicate, CancellationToken token) =>
                    _bookings.AsQueryable().FirstOrDefault(predicate.Compile()));

            // Setup PlaceAvailables query
            var startDate = booking.StartDate.Date;
            var endDate = booking.EndDate.Date;

            _mockContext.Setup(c => c.PlaceAvailables
                .Where(It.IsAny<Expression<Func<PlaceAvailable, bool>>>()))
                .Returns(_placeAvailables.Where(pa =>
                    pa.PlaceId == booking.PlaceId &&
                    pa.Date >= startDate &&
                    pa.Date <= endDate).AsQueryable());

            // Setup NotifyService
            _mockNotifyService.Setup(n => n.NotifyBookingStatusChangeAsync(bookingId, false, rejectReason))
                .Returns(Task.CompletedTask);

            // Setup SaveChangesAsync
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Setup for database transaction
            var dbContextTransaction = new Mock<IDbContextTransaction>();
            _mockContext.Setup(m => m.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextTransaction.Object);

            // Act
            var result = await _bookingService.UpdateBookingStatusAsync(bookingId, newStatus, currentRole, rejectReason);

            // Assert
            Assert.True(result);
            Assert.Equal(newStatus, booking.Status);

            // Verify notification was sent
            _mockNotifyService.Verify(n => n.NotifyBookingStatusChangeAsync(bookingId, false, rejectReason), Times.Once);

            // Verify transaction was committed
            dbContextTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBookingStatusAsync_WithInvalidTransition_ThrowsException()
        {
            // Arrange
            int bookingId = 2; // This booking is already Confirmed
            BookingStatus newStatus = BookingStatus.Pending; // Invalid transition from Confirmed to Pending
            string currentRole = "Landlord";

            // Setup booking retrieval
            var booking = _bookings.First(b => b.Id == bookingId);
            booking.Place = new Place { Owner = new ApplicationUser { Id = "owner1" } };

            _mockContext.Setup(c => c.Bookings
                .Include("Place.Owner")
                .FirstOrDefaultAsync(It.IsAny<Expression<Func<Booking, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expression<Func<Booking, bool>> predicate, CancellationToken token) =>
                    _bookings.AsQueryable().FirstOrDefault(predicate.Compile()));

            // Setup for database transaction
            var dbContextTransaction = new Mock<IDbContextTransaction>();
            _mockContext.Setup(m => m.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextTransaction.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _bookingService.UpdateBookingStatusAsync(bookingId, newStatus, currentRole));

            // Verify transaction was rolled back
            dbContextTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteBookingAsync_WithExistingBooking_ReturnsTrue()
        {
            // Arrange
            int bookingId = 1;
            var booking = _bookings.First(b => b.Id == bookingId);

            _mockContext.Setup(c => c.Bookings.FindAsync(bookingId))
                .ReturnsAsync(booking);

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _bookingService.DeleteBookingAsync(bookingId);

            // Assert
            Assert.True(result);

            // Verify that Remove was called
            _mockContext.Verify(c => c.Bookings.Remove(booking), Times.Once);

            // Verify that SaveChangesAsync was called
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteBookingAsync_WithNonExistentBooking_ReturnsFalse()
        {
            // Arrange
            int nonExistentBookingId = 999;

            _mockContext.Setup(c => c.Bookings.FindAsync(nonExistentBookingId))
                .ReturnsAsync((Booking)null);

            // Act
            var result = await _bookingService.DeleteBookingAsync(nonExistentBookingId);

            // Assert
            Assert.False(result);

            // Verify that Remove was not called
            _mockContext.Verify(c => c.Bookings.Remove(It.IsAny<Booking>()), Times.Never);

            // Verify that SaveChangesAsync was not called
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetAllBookingsAsync_WithoutFilters_ReturnsAllBookings()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;

            // Setup Count for pagination
            _mockContext.Setup(c => c.Bookings.CountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_bookings.Count);

            // Act
            var result = await _bookingService.GetAllBookingsAsync(null, null, null, page, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_bookings.Count, result.Count());
        }

        [Fact]
        public async Task GetAllBookingsAsync_WithStatusFilter_ReturnsFilteredBookings()
        {
            // Arrange
            string statusFilter = "Pending";
            int page = 1;
            int pageSize = 10;

            // Setup LINQ queries
            var filteredBookings = _bookings.Where(b => b.Status == BookingStatus.Pending).ToList();

            _mockContext.Setup(c => c.Bookings.Where(It.IsAny<Expression<Func<Booking, bool>>>()))
                .Returns(filteredBookings.AsQueryable());

            _mockContext.Setup(c => c.Bookings.Where(It.IsAny<Expression<Func<Booking, bool>>>()).CountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(filteredBookings.Count);

            // Act
            var result = await _bookingService.GetAllBookingsAsync(statusFilter, null, null, page, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.All(result, booking => Assert.Equal(BookingStatus.Pending, booking.Status));
        }

        [Fact]
        public async Task GetAllBookingsAsync_WithDateFilter_ReturnsFilteredBookings()
        {
            // Arrange
            DateTime startDateFilter = DateTime.UtcNow;
            DateTime endDateFilter = DateTime.UtcNow.AddDays(4);
            int page = 1;
            int pageSize = 10;

            // Setup LINQ queries
            var filteredBookings = _bookings.Where(b =>
                b.StartDate >= startDateFilter &&
                b.EndDate <= endDateFilter).ToList();

            _mockContext.Setup(c => c.Bookings.Where(It.IsAny<Expression<Func<Booking, bool>>>()))
                .Returns(filteredBookings.AsQueryable());

            _mockContext.Setup(c => c.Bookings.Where(It.IsAny<Expression<Func<Booking, bool>>>()).CountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(filteredBookings.Count);

            // Act
            var result = await _bookingService.GetAllBookingsAsync(null, startDateFilter, endDateFilter, page, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.All(result, booking =>
            {
                Assert.True(booking.StartDate >= startDateFilter);
                Assert.True(booking.EndDate <= endDateFilter);
            });
        }
    }
}