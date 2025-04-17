using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.Place;
using HomestayBookingAPI.Models;
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
                    var imageUrl = await _imageService.UploadImageAsync(imageFile);
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

        public async Task<List<PlaceDTO>> GetAllPlacesAsync()
        {
            try
            {
                var places = await _context.Places
                    .Include(p => p.Images)
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
                    Images = p.Images != null
                        ? p.Images.Select(i => new PlaceImageDTO
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl
                        })
                        .OrderBy(i => i.Id)
                        .ToList()
                        : new List<PlaceImageDTO>()
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
                    Images = p.Images != null
                            ? p.Images.Select(i => new PlaceImageDTO
                            {
                                Id = i.Id,
                                ImageUrl = i.ImageUrl
                            })
                            .OrderBy(i => i.Id)
                            .ToList()
                            : new List<PlaceImageDTO>()
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi : ", ex);
            }

        }

        public async Task<PlaceDTO> GetPlaceByID(int id)
        {
            var place =  await _context.Places
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
            if(place == null)
            {
                throw new Exception("Place not found");
            }
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
                Images = place.Images.Select(i => new PlaceImageDTO
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl
                })
                .OrderBy(i => i.Id)
                .ToList()
            };
        }

        public async Task<List<PlaceDTO>> GetSameCategoryPlaces(int id)
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
                    Images = p.Images != null
                        ? p.Images.Select(i => new PlaceImageDTO
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl
                        })
                        .OrderBy(i => i.Id) 
                        .ToList()
                        : new List<PlaceImageDTO>()
                }).ToList();

            }
            catch(Exception ex)
            {
                throw new Exception("Lỗi : ", ex);
            }

        }

        public async Task<List<PlaceDTO>> GetTopRatePlace(int limit)
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
                Images = p.Images.Select(i => new PlaceImageDTO
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl
                })
                .OrderBy(i => i.Id) // sap xep lai theo thu tu id
                .ToList()
            }).ToList();
        }

        public async Task<List<string>> UploadImagePlaceAsync(int placeId, List<IFormFile> images)
        {
            if (images == null || !images.Any())
            {
                _logger.LogError("Danh sách ảnh không được null hoặc rỗng.");
                throw new ArgumentException("Danh sách ảnh không được null hoặc rỗng.");
            }

            var place = await GetPlaceByID(placeId);
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
                    var imageUrl = await _imageService.UploadImageAsync(image);
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
