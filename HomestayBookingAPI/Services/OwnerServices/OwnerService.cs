using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.Place;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using HomestayBookingAPI.Services.ImageServices;
using HomestayBookingAPI.Services.PlaceServices;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace HomestayBookingAPI.Services.OwnerServices
{
    public class OwnerService : IOwnerService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPlaceService _placeService;
        private readonly IImageService _imageService;
        private readonly ILogger<OwnerService> _logger;

        public OwnerService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<OwnerService> logger, IPlaceService placeService, IImageService imageService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _placeService = placeService;
            _imageService = imageService;
        }

        public async Task<ApplicationUser> RegisterOwner(RegisterOwnerRequest ownerForm, RegisterPlaceRequest placeForm)
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var user = new ApplicationUser
                {
                    FullName = ownerForm.FullName,
                    IdentityCard = ownerForm.IdentityCard,
                    Email = ownerForm.Email,
                    PhoneNumber = ownerForm.PhoneNumber,
                    HomeAddress = ownerForm.HomeAddress,
                    UserName = ownerForm.Username,
                    Favourites = null
                }; // -> user

                var result = await _userManager.CreateAsync(user, ownerForm.Password);
                if (!result.Succeeded)
                {
                    _logger.LogError("Lỗi tạo tk: {Errors}", string.Join(", ", result.Errors));
                    throw new Exception($"Lỗi tạo tk: {string.Join(", ", result.Errors)}");
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "Landlord");
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Lỗi phân quyền {UserId}: {Errors}", user.Id, string.Join(", ", roleResult.Errors));
                    throw new Exception($"Lỗi phân quyền: {string.Join(", ", roleResult.Errors)}");
                }

                ////var placeImages = new List<PlaceImage>();
                ////if (placeForm.PlaceImages != null && placeForm.PlaceImages.Any())
                ////{
                ////    foreach (var imageFile in placeForm.PlaceImages)
                ////    {
                ////        var imageUrl = await _imageService.UploadImageAsync(imageFile);
                ////        if (imageUrl != null)
                ////        {
                ////            placeImages.Add(new PlaceImage { ImageUrl = imageUrl });
                ////        }
                ////        else
                ////        {
                ////            _logger.LogWarning("Lỗi tải ảnh.");
                ////        }
                ////    }
                ////} // -> Them anh 

                //if (!placeImages.Any())
                //{
                //    _logger.LogError("Không ảnh nào được thêm.");
                //    throw new Exception("Không ảnh nào được thêm.");
                //}

                var placeRequest = new PlaceRequest
                {
                    Name = placeForm.PlaceName,
                    Description = placeForm.PlaceDescription,
                    Address = placeForm.PlaceAddress,
                    Price = placeForm.PlacePrice,
                    OwnerId = user.Id,
                    Images = placeForm.PlaceImages,
                    Category = "homestay", //default
                    MaxGuests = 3, //default
                };

                
                var resultPlace = await _placeService.AddPlaceAsync(placeRequest);
                if (resultPlace == null)
                {
                    _logger.LogError("Lỗi tạo Homestay của chủ nhà mã {UserId}", user.Id);
                    throw new Exception("Tạo 0 thành công.");
                }

                transaction.Complete();
                _logger.LogInformation("User {UserId} and Place created successfully.", user.Id);
                return user;
            }
        }
    }
}
