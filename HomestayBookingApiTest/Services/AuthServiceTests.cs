using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using HomestayBookingAPI.Services.AuthService;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.JwtServices;
using HomestayBookingAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;

namespace HomestayBookingApiTest.Services;
public class AuthService_Tests
{
    private Mock<UserManager<ApplicationUser>> _userManagerMock;
    private Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private Mock<IJwtService> _jwtServiceMock;
    private AuthService _authService;

    public AuthService_Tests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var contextAccessor = new Mock<IHttpContextAccessor>();
        var userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            contextAccessor.Object,
            userClaimsPrincipalFactory.Object,
            null, null, null, null);

        _jwtServiceMock = new Mock<IJwtService>();

        _authService = new AuthService(_userManagerMock.Object, _signInManagerMock.Object, _jwtServiceMock.Object);
    }
    [Fact]
    public async Task RegisterUser_Success_ReturnsUser()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            FullName = "Nguyen Van A",
            IdentityCard = "123456789",
            Email = "test@example.com",
            PhoneNumber = "0123456789",
            HomeAddress = "Hanoi",
            Username = "nguyenvana",
            Password = "Password@123"
        };

        var newUser = new ApplicationUser
        {
            UserName = registerDto.Username,
            Email = registerDto.Email
        };

        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                        .ReturnsAsync(IdentityResult.Success); // Giả lập tạo người dùng thành công

        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Tenant"))
                        .ReturnsAsync(IdentityResult.Success); // -> Giả lập thêm tk mới vào role "Tenant"

        _signInManagerMock.Setup(s => s.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
                          .Returns(Task.CompletedTask); // -> Giả lập đăng nhập thành công

        var result = await _authService.RegisterUser(registerDto); // -> Gọi hàm đăng ký người dùng

        // Assert
        Assert.NotNull(result);
        Assert.Equal(registerDto.Email, result.Email); // -> trả về đúng email
    }
    [Fact]
    public async Task LoginUser_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            EmailorUsername = "testuser",
            Password = "Password@123"
        };

        var user = new ApplicationUser
        {
            UserName = "testuser",
            FullName = "Nguyen Van A",
            AvatarUrl = "http://example.com/avatar.jpg"
        };

        _userManagerMock.Setup(u => u.FindByNameAsync("testuser"))
                        .ReturnsAsync(user);

        _userManagerMock.Setup(u => u.CheckPasswordAsync(user, loginDto.Password))
                        .ReturnsAsync(true);

        _jwtServiceMock.Setup(j => j.GenerateSecurityToken(user))
                       .Returns("fake-jwt-token");

        // -> Tìm thấy user, pass đúng, trả về token giả

        // Act
        var result = await _authService.LoginUser(loginDto);

        Assert.NotNull(result);
        Assert.Equal("fake-jwt-token", result.Token); 
        Assert.Equal("Nguyen Van A", result.FullName);
    }
}
