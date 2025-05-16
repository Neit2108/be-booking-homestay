using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.Place;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using HomestayBookingAPI.Services.BookingServices;
using HomestayBookingAPI.Services.ImageServices;
using HomestayBookingAPI.Services.TopRatePlaceServices;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace HomestayBookingAPI.Services.PlaceServices
{
    public class PlaceService : IPlaceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly ITopRateService _topRateService;
        private readonly ILogger<PlaceService> _logger;
        private readonly IBookingService _bookingService;

        public PlaceService(ApplicationDbContext context, IImageService imageService, ILogger<PlaceService> logger, ITopRateService topRateService)
        {
            _context = context;
            _imageService = imageService;
            _logger = logger;
            _topRateService = topRateService;
        }

        public async Task<PlaceResponse> AddPlaceAsync(PlaceRequest placeRequest)
        {
            var placeImages = new List<PlaceImage>();
            if (placeRequest.Images != null && placeRequest.Images.Any())
            {
                foreach (var imageFile in placeRequest.Images)
                {
                    var imageUrl = await _imageService.UploadImageAsync(imageFile, "places");
                    if (imageUrl != null)
                    {
                        placeImages.Add(new PlaceImage { ImageUrl = imageUrl });
                    }
                    else
                    {
                        _logger.LogWarning("Lỗi tải ảnh.");
                    }
                }
            } // -> Them anh 
            if (!placeImages.Any())
            {
                _logger.LogError("Không ảnh nào được thêm.");
                throw new Exception("Không ảnh nào được thêm.");
            }
            var place = new Place
            {
                Name = placeRequest.Name,
                Address = placeRequest.Address,
                Category = placeRequest.Category,
                Description = placeRequest.Description,
                Price = placeRequest.Price,
                MaxGuests = placeRequest.MaxGuests,
                OwnerId = placeRequest.OwnerId,
                Status = PlaceStatus.Pending,
                Images = placeImages,
            };
            if (place == null)
            {
                throw new Exception("Place is null");
            }

            var validationPlace = new ValidationContext(place);
            var validationResult = new List<ValidationResult>();
            if (!Validator.TryValidateObject(place, validationPlace, validationResult, true))
            {
                throw new Exception("Dữ liệu không hợp lệ");
            }
            try
            {
                await _context.Places.AddAsync(place);
                await _context.SaveChangesAsync();
                return new PlaceResponse
                {
                    Id = place.Id,
                    Name = place.Name,
                    Address = place.Address,
                    Rating = place.Rating,
                    NumOfRating = place.NumOfRating,
                    Category = place.Category,
                    Description = place.Description,
                    Price = place.Price,
                    MaxGuests = place.MaxGuests,
                    Status = place.Status.ToString(),
                    Images = place.Images != null
                        ? place.Images.Select(i => new PlaceImageDTO
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl
                        })
                        .OrderBy(i => i.Id)
                        .ToList()
                        : new List<PlaceImageDTO>()
                };
            }
            catch (DbUpdateException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<PlaceDTO>> GetAllPlacesAsync(string userId)
        {
            try
            {
                var places = await _context.Places
                    .Include(p => p.Images)
                    .ToListAsync();
                var userFavorites = await _context.Favourites
                    .Where(f => f.UserId == userId)
                    .Select(f => f.PlaceId)
                    .ToListAsync();
                foreach(var x in userFavorites)
                {
                    _logger.LogDebug("userFavourite " + x + "\n");
                }
                
                return places.Select(p => new PlaceDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Rating = p.Rating,
                    NumOfRating = p.NumOfRating,
                    Category = p.Category,
                    Description = p.Description,
                    Price = p.Price,
                    MaxGuests = p.MaxGuests,
                    Status = p.Status.ToString(),
                    Images = p.Images != null
                        ? p.Images.Select(i => new PlaceImageDTO
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl
                        })
                        .OrderBy(i => i.Id)
                        .ToList()
                        : new List<PlaceImageDTO>(),
                    IsFavourite = userFavorites.Contains(p.Id) 
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi : ", ex);
            }
        }

        public async Task<List<PlaceDTO>> GetAllPlacesOfLandlord(string landlordId)
        {
            try
            {
                var places = await _context.Places
                .Include(p => p.Images)
                .Where(p => p.OwnerId == landlordId)
                .ToListAsync();
                var userFavorites = await _context.Favourites
                    .Where(f => f.UserId == landlordId)
                    .Select(f => f.PlaceId)
                    .ToListAsync();
                return places.Select(p => new PlaceDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Rating = p.Rating,
                    NumOfRating = p.NumOfRating,
                    Category = p.Category,
                    Description = p.Description,
                    Price = p.Price,
                    MaxGuests = p.MaxGuests,
                    Status = p.Status.ToString(),
                    Images = p.Images != null
                            ? p.Images.Select(i => new PlaceImageDTO
                            {
                                Id = i.Id,
                                ImageUrl = i.ImageUrl
                            })
                            .OrderBy(i => i.Id)
                            .ToList()
                            : new List<PlaceImageDTO>(),
                    IsFavourite = userFavorites.Contains(p.Id)
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi : ", ex);
            }

        }

        public async Task<PlaceDTO> GetPlaceByID(int id, string userId)
        {
            var place =  await _context.Places
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
            if(place == null)
            {
                throw new Exception("Place not found");
            }

            var userFavorites = await _context.Favourites
                .Where(f => f.UserId == userId)
                .Select(f => f.PlaceId)
                .ToListAsync();

            return new PlaceDTO
            {
                Id = place.Id,
                Name = place.Name,
                Address = place.Address,
                Rating = place.Rating,
                NumOfRating = place.NumOfRating,
                Category = place.Category,
                Description = place.Description,
                Price = place.Price,
                MaxGuests = place.MaxGuests,
                Status = place.Status.ToString(),
                Images = place.Images.Select(i => new PlaceImageDTO
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl
                })
                .OrderBy(i => i.Id)
                .ToList(),
                IsFavourite = userFavorites.Contains(place.Id)
            };
        }

        public async Task<List<PlaceDTO>> GetSameCategoryPlaces(int id, string userId)
        {
            
            try
            {
                var this_place = await _context.Places.FindAsync(id);

                if(this_place == null)
                {
                    throw new Exception("Lew lew");
                }

                var sameCategoryPlaces = await _context.Places
                    .Where(p => p.Category == this_place.Category && p.Id != id)
                    .Include(p => p.Images)
                    .Take(4)
                    .ToListAsync();

                var userFavorites = await _context.Favourites
                    .Where(f => f.UserId == userId)
                    .Select(f => f.PlaceId)
                    .ToListAsync();

                return sameCategoryPlaces.Select(p => new PlaceDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Rating = p.Rating,
                    NumOfRating = p.NumOfRating,
                    Category = p.Category,
                    Description = p.Description,
                    Price = p.Price,
                    MaxGuests = p.MaxGuests,
                    Status = p.Status.ToString(),
                    Images = p.Images != null
                        ? p.Images.Select(i => new PlaceImageDTO
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl
                        })
                        .OrderBy(i => i.Id) 
                        .ToList()
                        : new List<PlaceImageDTO>(),
                    IsFavourite = userFavorites.Contains(p.Id)
                }).ToList();

            }
            catch(Exception ex)
            {
                throw new Exception("Lỗi : ", ex);
            }

        }

        public async Task<List<PlaceDTO>> GetTopRatePlace(int limit, string userId)
        {
            var lastUpdate = await _context.TopRatePlaces
                .OrderByDescending(trp => trp.LastUpdated)
                .Select(trp => trp.LastUpdated)
                .FirstOrDefaultAsync();

            bool needUpdate = lastUpdate == default || lastUpdate.AddHours(5) < DateTime.Now;

            if (needUpdate)
            {
                await _topRateService.UpdateTopRateAsync(limit);
            }

            var topRatePlaceID = await _context.TopRatePlaces
                .OrderBy(trp => trp.Rank)
                .Select(trp => trp.PlaceId)
                .Take(limit)
                .ToListAsync(); // lay id cua top rate places

            var places = await _context.Places
                .Include(p => p.Images)
                .Where(p => topRatePlaceID.Contains(p.Id))
                .ToListAsync(); // lay thong tin cua top rate places

            var userFavorites = await _context.Favourites
                .Where(f => f.UserId == userId)
                .Select(f => f.PlaceId)
                .ToListAsync();

            places = topRatePlaceID.Select(id => places.First(p => p.Id == id)).ToList(); // sap xep lai theo thu tu top rate

            return places.Select(p => new PlaceDTO
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                Rating = p.Rating,
                NumOfRating = p.NumOfRating,
                Category = p.Category,
                Description = p.Description,
                Price = p.Price,
                MaxGuests = p.MaxGuests,
                Status = p.Status.ToString(),
                Images = p.Images.Select(i => new PlaceImageDTO
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl
                })
                .OrderBy(i => i.Id) // sap xep lai theo thu tu id
                .ToList(),
                IsFavourite = userFavorites.Contains(p.Id)
            }).ToList();
        }

        public async Task<bool> UpdatePlaceStatusAsync(UpdatePlaceStatusRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var place = await _context.Places.FindAsync(request.PlaceId);
                if (place == null)
                {
                    return false;
                }

                // Nếu chuyển từ Active sang Inactive
                if (place.Status == PlaceStatus.Active && request.NewStatus == PlaceStatus.Inactive)
                {
                    // Kiểm tra có booking nào đã confirmed mà chưa kết thúc không
                    var confirmedBookings = await _context.Bookings
                        .Where(b => b.PlaceId == request.PlaceId &&
                                   b.Status == BookingStatus.Confirmed &&
                                   b.EndDate > DateTime.UtcNow)
                        .OrderByDescending(b => b.EndDate)
                        .ToListAsync();

                    if (confirmedBookings.Any())
                    {
                        // Nếu ngày bắt đầu Inactive sớm hơn ngày kết thúc của booking cuối cùng
                        var lastBookingEndDate = confirmedBookings.First().EndDate;

                        if (request.InactiveFrom == null || request.InactiveFrom < lastBookingEndDate)
                        {
                            request.InactiveFrom = lastBookingEndDate.AddDays(1);
                            _logger.LogInformation($"Adjusted InactiveFrom date to {request.InactiveFrom} due to existing bookings");
                        }
                    }

                    // Cập nhật trạng thái Place
                    place.Status = request.NewStatus;
                    _context.Places.Update(place);

                    // Cập nhật PlaceAvailable
                    var startDate = request.InactiveFrom ?? DateTime.UtcNow;
                    var endDate = request.InactiveTo ?? DateTime.MaxValue.AddYears(-100); // Giới hạn để tránh lỗi vượt quá datetime

                    // Vấn đề có thể ở đây - chúng ta không nên tạo mới vô số bản ghi
                    // Chỉ tạo từ ngày hiện tại đến 1 thời điểm hợp lý trong tương lai (ví dụ: 3 năm)
                    // hoặc đến inactiveTo nếu có
                    DateTime maxFutureDate = startDate.AddYears(3); // Giới hạn 3 năm vào tương lai
                    if (request.InactiveTo.HasValue && request.InactiveTo.Value < maxFutureDate)
                    {
                        maxFutureDate = request.InactiveTo.Value;
                    }

                    // Lấy các PlaceAvailable hiện có trong khoảng thời gian
                    var placeAvailables = await _context.PlaceAvailables
                        .Where(pa => pa.PlaceId == request.PlaceId &&
                                  pa.Date >= startDate &&
                                  pa.Date <= maxFutureDate)
                        .ToListAsync();

                    // Cập nhật các bản ghi hiện có
                    foreach (var pa in placeAvailables)
                    {
                        pa.IsAvailable = false;
                        _context.PlaceAvailables.Update(pa);
                    }

                    // Tạo mới các bản ghi nếu không tồn tại
                    // Lấy danh sách các ngày cần tạo mới
                    var existingDates = placeAvailables.Select(pa => pa.Date.Date).ToHashSet();

                    // Chỉ tạo bản ghi cho một khoảng thời gian hợp lý - không quá 365 ngày từ startDate
                    DateTime limitEndDate = startDate.AddDays(365);
                    if (maxFutureDate < limitEndDate)
                    {
                        limitEndDate = maxFutureDate;
                    }

                    // Sử dụng hàm helper để tạo danh sách ngày từ startDate đến limitEndDate
                    var allDates = GenerateDateRange(startDate.Date, limitEndDate.Date);

                    var newEntries = allDates
                        .Where(d => !existingDates.Contains(d))
                        .Select(d => new PlaceAvailable
                        {
                            PlaceId = request.PlaceId,
                            Date = d,
                            IsAvailable = false
                        })
                        .ToList();

                    if (newEntries.Any())
                    {
                        await _context.PlaceAvailables.AddRangeAsync(newEntries);
                    }

                    // Xử lý các booking đang pending
                    var pendingBookings = await _context.Bookings
                        .Where(b => b.PlaceId == request.PlaceId &&
                                  b.Status == BookingStatus.Pending &&
                                  ((b.StartDate >= startDate && (request.InactiveTo == null || b.StartDate <= endDate)) ||
                                   (b.EndDate >= startDate && (request.InactiveTo == null || b.EndDate <= endDate))))
                        .ToListAsync();

                    // Tự động từ chối các booking pending nếu trùng với thời gian inactive
                    foreach (var booking in pendingBookings)
                    {
                        await _bookingService.UpdateBookingStatusAsync(booking.Id, BookingStatus.Cancelled, "Admin", "Homestay ngừng hoạt động");
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                // Ngược lại nếu từ Inactive sang Active
                else if (place.Status == PlaceStatus.Inactive && request.NewStatus == PlaceStatus.Active)
                {
                    // Cập nhật trạng thái Place
                    place.Status = request.NewStatus;
                    _context.Places.Update(place);

                    // Lấy các PlaceAvailable từ hiện tại trở đi mà IsAvailable = false
                    var futureUnavailableDates = await _context.PlaceAvailables
                        .Where(pa => pa.PlaceId == request.PlaceId &&
                                  pa.Date >= DateTime.UtcNow &&
                                  !pa.IsAvailable)
                        .ToListAsync();

                    //foreach (var pa in futureUnavailableDates)
                    //{
                    //    pa.IsAvailable = true;
                    //    _context.PlaceAvailables.Update(pa);
                    //}
                    // Xóa các bản ghi không cần thiết
                    _context.PlaceAvailables.RemoveRange(futureUnavailableDates);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                else
                {
                    // Chỉ đơn giản cập nhật trạng thái nếu không phải trường hợp đặc biệt
                    place.Status = request.NewStatus;
                    _context.Places.Update(place);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error updating place status for PlaceId {request.PlaceId}");
                throw;
            }
        }

        private IEnumerable<DateTime> GenerateDateRange(DateTime startDate, DateTime endDate)
        {
            // Giới hạn số ngày tối đa để tránh vòng lặp vô tận
            const int MaxDaysToGenerate = 365;

            int daysCount = 0;
            for (var date = startDate; date <= endDate && daysCount < MaxDaysToGenerate; date = date.AddDays(1))
            {
                yield return date;
                daysCount++;
            }
        }

        public async Task<List<string>> UploadImagePlaceAsync(int placeId, List<IFormFile> images)
        {
            if (images == null || !images.Any())
            {
                _logger.LogError("Danh sách ảnh không được null hoặc rỗng.");
                throw new ArgumentException("Danh sách ảnh không được null hoặc rỗng.");
            }

            var place = await _context.Places.FindAsync(placeId);
            if (place == null)
            {
                _logger.LogError("Place not found");
                throw new Exception("Place not found");
            }
            
            var uploadedImages = new List<string>();
            foreach (var image in images)
            {
                try
                {
                    var imageUrl = await _imageService.UploadImageAsync(image, "places");
                    if (imageUrl == null)
                    {
                        _logger.LogError("Image not uploaded");
                        throw new Exception("Image not uploaded");
                        continue;
                    }
                    var placeImage = new PlaceImage
                    {
                        PlaceId = placeId,
                        ImageUrl = imageUrl
                    };
                    await _context.PlaceImages.AddAsync(placeImage);
                    await _context.SaveChangesAsync();
                    uploadedImages.Add(imageUrl);
                    _logger.LogInformation($"Image {imageUrl} uploaded successfully");
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            if (!uploadedImages.Any())
            {
                _logger.LogError("Không có ảnh nào được upload thành công.");
                throw new Exception("Không có ảnh nào được upload thành công.");
            }

            return uploadedImages;
        }
    }
}
