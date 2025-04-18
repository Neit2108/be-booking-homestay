using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using HomestayBookingAPI.Services.ProfileServices;
using HomestayBookingAPI.Services.UserServices;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.DTOs;

namespace HomestayBookingApiTest.Services;
public class ProfileService_Tests
{
    // Tạo dependency giả
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly ProfileService _profileService;

    public ProfileService_Tests()
    {
        _userServiceMock = new Mock<IUserService>();

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        _profileService = new ProfileService(_userServiceMock.Object, _userManagerMock.Object);
    }

    [Fact] // test method
    public async Task UpdateProfileAsync_UserExists_ReturnsTrue()
    {
        // Arrange
        string userId = "user123";
        var existingUser = new ApplicationUser { Id = userId, UserName = "testuser" };

        _userServiceMock.Setup(s => s.GetUserByID(userId))
                        .ReturnsAsync(existingUser);

        _userManagerMock.Setup(m => m.UpdateAsync(existingUser))
                        .ReturnsAsync(IdentityResult.Success);

        var profileDto = new ProfileDTO
        {
            FullName = "Nguyen Van B",
            PhoneNumber = "0999999999",
            Address = "HCMC",
            BirthDate = new DateTime(1999, 1, 1),
            Gender = 0,
            Bio = "Bio test",
            IdentityCard = "123456789"
        };

        // Act
        var result = await _profileService.UpdateProfileAsync(userId, profileDto);

        // Assert
        Assert.True(result);
        Assert.Equal("Nguyen Van B", existingUser.FullName);
        Assert.Equal("HCMC", existingUser.HomeAddress);
    }
    [Fact]
    public async Task UpdateProfileAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        string userId = "notfound";
        _userServiceMock.Setup(s => s.GetUserByID(userId))
                        .ReturnsAsync((ApplicationUser)null); // null user

        var profileDto = new ProfileDTO
        {
            FullName = "Test",
            PhoneNumber = "000000000",
            Address = "Nowhere"
        };

        // Act
        var result = await _profileService.UpdateProfileAsync(userId, profileDto);

        // Assert
        Assert.False(result);
    }
}