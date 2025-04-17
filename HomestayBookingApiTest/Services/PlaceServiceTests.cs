using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.ImageServices;
using HomestayBookingAPI.Services.PlaceServices;
using HomestayBookingAPI.Services.TopRatePlaceServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HomestayBookingAPI.Tests.Services
{
    public class PlaceServiceTests
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<IImageService> _mockImageService;
        private readonly Mock<ITopRateService> _mockTopRateService;
        private readonly Mock<ILogger<PlaceService>> _mockLogger;
        private readonly PlaceService _placeService;
        private readonly List<Place> _places;
        private readonly List<PlaceImage> _placeImages;
        private readonly List<TopRatePlaces> _topRatePlaces;

        public PlaceServiceTests()
        {
            // Sample data
            _places = new List<Place>
            {
                new Place
                {
                    Id = 1,
                    Name = "Beach House",
                    Address = "123 Beach Rd",
                    Rating = 4.8,
                    NumOfRating = 25,
                    Category = "beach",
                    Description = "Beautiful beach house",
                    Price = 150.0,
                    MaxGuests = 4,
                    OwnerId = "owner1",
                    Images = new List<PlaceImage>()
                },
                new Place
                {
                    Id = 2,
                    Name = "Mountain Cabin",
                    Address = "456 Mountain Rd",
                    Rating = 4.6,
                    NumOfRating = 18,
                    Category = "mountain",
                    Description = "Cozy mountain cabin",
                    Price = 120.0,
                    MaxGuests = 3,
                    OwnerId = "owner1",
                    Images = new List<PlaceImage>()
                },
                new Place
                {
                    Id = 3,
                    Name = "City Apartment",
                    Address = "789 Main St",
                    Rating = 4.5,
                    NumOfRating = 30,
                    Category = "city",
                    Description = "Modern city apartment",
                    Price = 200.0,
                    MaxGuests = 2,
                    OwnerId = "owner2",
                    Images = new List<PlaceImage>()
                }
            };

            _placeImages = new List<PlaceImage>
            {
                new PlaceImage { Id = 1, PlaceId = 1, ImageUrl = "https://example.com/beach1.jpg" },
                new PlaceImage { Id = 2, PlaceId = 1, ImageUrl = "https://example.com/beach2.jpg" },
                new PlaceImage { Id = 3, PlaceId = 2, ImageUrl = "https://example.com/mountain1.jpg" },
                new PlaceImage { Id = 4, PlaceId = 3, ImageUrl = "https://example.com/city1.jpg" }
            };

            _topRatePlaces = new List<TopRatePlaces>
            {
                new TopRatePlaces { Id = 1, PlaceId = 1, Rating = 4.8, Rank = 1, LastUpdated = DateTime.UtcNow.AddHours(-2) },
                new TopRatePlaces { Id = 2, PlaceId = 2, Rating = 4.6, Rank = 2, LastUpdated = DateTime.UtcNow.AddHours(-2) }
            };

            // Link images to places
            _places[0].Images.Add(_placeImages[0]);
            _places[0].Images.Add(_placeImages[1]);
            _places[1].Images.Add(_placeImages[2]);
            _places[2].Images.Add(_placeImages[3]);

            // Setup mocks
            _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            _mockImageService = new Mock<IImageService>();
            _mockTopRateService = new Mock<ITopRateService>();
            _mockLogger = new Mock<ILogger<PlaceService>>();

            // Setup mock DbSets
            var mockPlacesDbSet = GetMockDbSet(_places);
            _mockContext.Setup(c => c.Places).Returns(mockPlacesDbSet);

            var mockImagesDbSet = GetMockDbSet(_placeImages);
            _mockContext.Setup(c => c.PlaceImages).Returns(mockImagesDbSet);

            var mockTopRatePlacesDbSet = GetMockDbSet(_topRatePlaces);
            _mockContext.Setup(c => c.TopRatePlaces).Returns(mockTopRatePlacesDbSet);

            // Create service
            _placeService = new PlaceService(
                _mockContext.Object,
                _mockImageService.Object,
                _mockLogger.Object,
                _mockTopRateService.Object);
        }

        [Fact]
        public async Task GetPlaceByID_WithValidId_ReturnsPlaceDTO()
        {
            // Arrange
            int placeId = 1;

            // Setup FindAsync to return a place
            _mockContext.Setup(c => c.Places.FindAsync(placeId))
                .ReturnsAsync(_places.FirstOrDefault(p => p.Id == placeId));

            // Act
            var result = await _placeService.GetPlaceByID(placeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(placeId, result.Id);
            Assert.Equal("Beach House", result.Name);
            Assert.Equal(4.8, result.Rating);
            Assert.Equal(2, result.Images.Count); // Should have 2 images
        }

        [Fact]
        public async Task GetPlaceByID_WithInvalidId_ThrowsException()
        {
            // Arrange
            int nonExistentPlaceId = 999;

            // Setup FindAsync to return null
            _mockContext.Setup(c => c.Places.FindAsync(nonExistentPlaceId))
                .ReturnsAsync((Place)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _placeService.GetPlaceByID(nonExistentPlaceId));
        }

        [Fact]
        public async Task GetAllPlacesAsync_ReturnsAllPlaces()
        {
            // Arrange
            // Setup is already done in constructor

            // Act
            var result = await _placeService.GetAllPlacesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count); // Should return all 3 places

            // Verify the data is correctly mapped
            Assert.Contains(result, p => p.Id == 1 && p.Name == "Beach House");
            Assert.Contains(result, p => p.Id == 2 && p.Name == "Mountain Cabin");
            Assert.Contains(result, p => p.Id == 3 && p.Name == "City Apartment");
        }

        [Fact]
        public async Task GetAllPlacesOfLandlord_WithValidOwnerId_ReturnsOwnerPlaces()
        {
            // Arrange
            string ownerId = "owner1";

            // Setup Where query
            _mockContext.Setup(c => c.Places
                .Where(It.IsAny<Expression<Func<Place, bool>>>()))
                .Returns<Expression<Func<Place, bool>>>(expr =>
                    _places.AsQueryable().Where(expr));

            // Act
            var result = await _placeService.GetAllPlacesOfLandlord(ownerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Owner1 has 2 places
            Assert.All(result, place => Assert.Equal(ownerId, _places.First(p => p.Id == place.Id).OwnerId));
        }

        [Fact]
        public async Task GetSameCategoryPlaces_ReturnsPlacesWithSameCategory()
        {
            // Arrange
            int placeId = 1; // Beach House

            // Setup FindAsync
            _mockContext.Setup(c => c.Places.FindAsync(placeId))
                .ReturnsAsync(_places.FirstOrDefault(p => p.Id == placeId));

            // Setup Where query with category filter
            _mockContext.Setup(c => c.Places
                .Where(It.IsAny<Expression<Func<Place, bool>>>()))
                .Returns<Expression<Func<Place, bool>>>(expr =>
                    _places.AsQueryable().Where(expr));

            // Act
            var result = await _placeService.GetSameCategoryPlaces(placeId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // No other places have the "beach" category in our test data
        }

        [Fact]
        public async Task GetTopRatePlace_WithNoLastUpdateOrExpired_UpdatesTopPlaces()
        {
            // Arrange
            int limit = 3;

            // Setup LastUpdated to be over 5 hours ago
            _topRatePlaces[0].LastUpdated = DateTime.UtcNow.AddHours(-6);

            // Setup OrderByDescending to return the ordered topRatePlaces
            _mockContext.Setup(c => c.TopRatePlaces
                .OrderByDescending(It.IsAny<Expression<Func<TopRatePlaces, DateTime>>>()))
                .Returns<Expression<Func<TopRatePlaces, DateTime>>>(expr =>
                    _topRatePlaces.AsQueryable().OrderByDescending(expr));

            // Setup FirstOrDefaultAsync to return the first LastUpdated
            _mockContext.Setup(c => c.TopRatePlaces
                .OrderByDescending(It.IsAny<Expression<Func<TopRatePlaces, DateTime>>>())
                .Select(It.IsAny<Expression<Func<TopRatePlaces, DateTime>>>())
                .FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_topRatePlaces.OrderByDescending(t => t.LastUpdated).Select(t => t.LastUpdated).FirstOrDefault());

            // Setup TopRateService to update
            _mockTopRateService.Setup(s => s.UpdateTopRateAsync(limit))
                .Returns(Task.CompletedTask);

            // Setup to return top rated places by ID
            _mockContext.Setup(c => c.TopRatePlaces
                .OrderBy(It.IsAny<Expression<Func<TopRatePlaces, int>>>())
                .Select(It.IsAny<Expression<Func<TopRatePlaces, int>>>())
                .Take(limit)
                .ToListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_topRatePlaces.OrderBy(t => t.Rank).Select(t => t.PlaceId).Take(limit).ToList());

            // Setup to filter places by IDs
            _mockContext.Setup(c => c.Places
                .Where(It.IsAny<Expression<Func<Place, bool>>>()))
                .Returns<Expression<Func<Place, bool>>>(expr =>
                    _places.AsQueryable().Where(expr));

            // Act
            var result = await _placeService.GetTopRatePlace(limit);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(limit > _places.Count ? _places.Count : limit, result.Count);

            // Verify TopRateService was called to update
            _mockTopRateService.Verify(s => s.UpdateTopRateAsync(limit), Times.Once);
        }

        [Fact]
        public async Task AddPlaceAsync_WithValidPlace_ReturnsAddedPlace()
        {
            // Arrange
            var place = new Place
            {
                Name = "New Place",
                Address = "New Address",
                Rating = 5.0,
                NumOfRating = 1,
                Category = "new",
                Description = "New description",
                Price = 250.0,
                MaxGuests = 5,
                OwnerId = "owner3"
            };

            // Setup AddAsync
            //_mockContext.Setup(c => c.Places.AddAsync(place, It.IsAny<CancellationToken>()))
            //    .ReturnsAsync((EntityEntry<Place>)null); // We don't need a real EntityEntry for the test
            var mockEntityEntry = new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Place>>();
            _mockContext.Setup(c => c.Places.AddAsync(It.IsAny<Place>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockEntityEntry.Object);

            // Setup SaveChangesAsync
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _placeService.AddPlaceAsync(place);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(place.Name, result.Name);
            Assert.Equal(place.Price, result.Price);

            // Verify AddAsync was called
            _mockContext.Verify(c => c.Places.AddAsync(place, It.IsAny<CancellationToken>()), Times.Once);

            // Verify SaveChangesAsync was called
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddPlaceAsync_WithNullPlace_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _placeService.AddPlaceAsync(null));
        }

        [Fact]
        public async Task AddPlaceAsync_WithInvalidPlace_ThrowsException()
        {
            // Arrange
            var invalidPlace = new Place(); // Missing required fields

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _placeService.AddPlaceAsync(invalidPlace));
        }

        [Fact]
        public async Task UploadImagePlaceAsync_WithValidImagesAndPlaceId_ReturnsImageUrls()
        {
            // Arrange
            int placeId = 1;
            var mockFormFiles = new List<IFormFile>
            {
                CreateMockFormFile("image1.jpg", "image/jpeg"),
                CreateMockFormFile("image2.png", "image/png")
            };

            string[] uploadedUrls = new string[]
            {
                "https://localhost:7284/uploads/image1.jpg",
                "https://localhost:7284/uploads/image2.png"
            };

            // Setup GetPlaceByID
            _mockContext.Setup(c => c.Places.FindAsync(placeId))
                .ReturnsAsync(_places.FirstOrDefault(p => p.Id == placeId));

            // Setup ImageService
            //for (int i = 0; i < mockFormFiles.Count; i++)
            //{
            //    int index = i; // Capture for closure
            //    _mockImageService.Setup(s => s.UploadImageAsync(mockFormFiles[index]))
            //        .ReturnsAsync(uploadedUrls[index]);
            //}

            // Setup AddAsync for PlaceImages
            var mockEntityEntry = new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PlaceImage>>();
            _mockContext.Setup(c => c.PlaceImages.AddAsync(It.IsAny<PlaceImage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockEntityEntry.Object);

            // Setup SaveChangesAsync
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _placeService.UploadImagePlaceAsync(placeId, mockFormFiles);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockFormFiles.Count, result.Count);
            Assert.Equal(uploadedUrls, result);

            // Verify UploadImageAsync was called for each file
            //for (int i = 0; i < mockFormFiles.Count; i++)
            //{
            //    _mockImageService.Verify(s => s.UploadImageAsync(mockFormFiles[i]), Times.Once);
            //}

            // Verify AddAsync was called for each image
            _mockContext.Verify(c => c.PlaceImages.AddAsync(It.IsAny<PlaceImage>(), It.IsAny<CancellationToken>()),
                Times.Exactly(mockFormFiles.Count));

            // Verify SaveChangesAsync was called for each image
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Exactly(mockFormFiles.Count));
        }

        [Fact]
        public async Task UploadImagePlaceAsync_WithNullImages_ThrowsException()
        {
            // Arrange
            int placeId = 1;
            List<IFormFile> nullFormFiles = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _placeService.UploadImagePlaceAsync(placeId, nullFormFiles));
        }

        [Fact]
        public async Task UploadImagePlaceAsync_WithEmptyImages_ThrowsException()
        {
            // Arrange
            int placeId = 1;
            var emptyFormFiles = new List<IFormFile>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _placeService.UploadImagePlaceAsync(placeId, emptyFormFiles));
        }

        // Helper methods
        private static DbSet<T> GetMockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockDbSet = new Mock<DbSet<T>>();

            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            mockDbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(data.Add);
            mockDbSet.Setup(d => d.AddRange(It.IsAny<IEnumerable<T>>()))
                .Callback<IEnumerable<T>>(items => data.AddRange(items));
            mockDbSet.Setup(d => d.Remove(It.IsAny<T>()))
                .Callback<T>(item => data.Remove(item));

            mockDbSet.Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] ids) => {
                    var id = ids[0];
                    return data.FirstOrDefault(d => GetId(d).Equals(id));
                });

            return mockDbSet.Object;
        }

        private static object GetId<T>(T entity)
        {
            var property = typeof(T).GetProperty("Id");
            return property?.GetValue(entity);
        }

        private IFormFile CreateMockFormFile(string fileName, string contentType)
        {
            var content = "dummy content";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.ContentType).Returns(contentType);
            formFile.Setup(f => f.Length).Returns(stream.Length);
            formFile.Setup(f => f.OpenReadStream()).Returns(stream);
            formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream targetStream, CancellationToken token) => {
                    return stream.CopyToAsync(targetStream);
                });

            return formFile.Object;
        }
    }

    // This is a mock for EntityEntry which EF Core would normally return
    public class EntityEntry<T> where T : class
    {
    }
}