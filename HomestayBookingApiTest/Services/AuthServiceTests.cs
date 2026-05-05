using Moq;
using Microsoft.AspNetCore.Identity;
using HomestayBookingAPI.Services.AuthService;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.JwtServices;
using HomestayBookingAPI.DTOs;
using Microsoft.AspNetCore.Http;
using HomestayBookingAPI.Services.WalletServices;
using HomestayBookingAPI.Services.NotifyServices;

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
        var walletServiceMock = new Mock<IWalletService>();

        var contextAccessor = new Mock<IHttpContextAccessor>();
        var userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            contextAccessor.Object,
            userClaimsPrincipalFactory.Object,
            null, null, null, null);

        _jwtServiceMock = new Mock<IJwtService>();
        var _notifyServiceMock = new Mock<INotifyService>();

        //_authService = new AuthService(_userManagerMock.Object, _signInManagerMock.Object, _jwtServiceMock.Object, walletServiceMock.Object, _notifyServiceMock.Object);
    }

    /*
        Register :
            -chưa test dòng return null -> bao phủ dòng độ 85%
            -chưa test trường hợp tạo người dùng thất bại -> bao phủ nhánh 50%
    
        Login : 
            -chưa test user = null, password = false, isEmail = true -> bao phủ dòng độ 60
            -chưa test trường hợp user không tồn tại -> bao phủ nhánh 50%
     
     */

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
        }; // tạo 1 user mới

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
    public async Task RegisterUser_FailedCreate_ReturnsNull()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            FullName = "Nguyen Van B",
            IdentityCard = "987654321",
            Email = "fail@example.com",
            PhoneNumber = "0999999999",
            HomeAddress = "HCMC",
            Username = "failuser",
            Password = "InvalidPassword123"
        };

        // Setup: giả lập tạo người dùng thất bại
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Tạo thất bại" }));

        // Act
        var result = await _authService.RegisterUser(registerDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterUser_ThrowsException_ReturnsNull()
    {
        var registerDto = new RegisterDTO
        {
            FullName = "Throw Error",
            Email = "error@example.com",
            Username = "erroruser",
            Password = "errorpass"
        };

        // Giả lập CreateAsync ném lỗi
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                        .ThrowsAsync(new Exception("fake exception"));

        var result = await _authService.RegisterUser(registerDto);

        Assert.Null(result); // => Phải nhảy vào catch
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

    [Fact]
    public async Task LoginUser_UserNotFound_ReturnsNull()
    {
        // Arrange
        var loginDto = new LoginDTO { EmailorUsername = "notfound", Password = "any" };

        _userManagerMock.Setup(u => u.FindByNameAsync(loginDto.EmailorUsername))
                        .ReturnsAsync((ApplicationUser)null); // User không tồn tại

        // Act
        var result = await _authService.LoginUser(loginDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginUser_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var loginDto = new LoginDTO { EmailorUsername = "testuser", Password = "wrongpassword" };
        var user = new ApplicationUser { UserName = "testuser", FullName = "Nguyen Van A" };

        _userManagerMock.Setup(u => u.FindByNameAsync(loginDto.EmailorUsername))
                        .ReturnsAsync(user); // User tồn tại

        _userManagerMock.Setup(u => u.CheckPasswordAsync(user, loginDto.Password))
                        .ReturnsAsync(false); // Sai mật khẩu

        // Act
        var result = await _authService.LoginUser(loginDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginUser_ThrowsException_ReturnsNull()
    {
        var loginDto = new LoginDTO
        {
            EmailorUsername = "erroruser",
            Password = "errorpass"
        };

        // Giả lập FindByNameAsync ném lỗi
        _userManagerMock.Setup(u => u.FindByNameAsync(loginDto.EmailorUsername))
                        .ThrowsAsync(new Exception("fake exception"));

        var result = await _authService.LoginUser(loginDto);

        Assert.Null(result); // => Phải nhảy vào catch
    }


}
