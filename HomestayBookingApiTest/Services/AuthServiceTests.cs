using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.AuthService;
using HomestayBookingAPI.Services.JwtServices;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace HomestayBookingAPI.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Set up UserManager mock
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Set up SignInManager mock 
            var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object, contextAccessorMock.Object, userPrincipalFactoryMock.Object, null, null, null, null);

            // Set up JwtService mock
            _mockJwtService = new Mock<IJwtService>();

            // Create service
            _authService = new AuthService(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockJwtService.Object);
        }

        [Fact]
        public async Task RegisterUser_WithValidData_ReturnsUser()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                FullName = "John Doe",
                IdentityCard = "123456789012",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                HomeAddress = "123 Main St",
                Username = "johndoe",
                Password = "Password123!"
            };

            var createdUser = new ApplicationUser
            {
                Id = "user1",
                FullName = registerDto.FullName,
                IdentityCard = registerDto.IdentityCard,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                HomeAddress = registerDto.HomeAddress,
                UserName = registerDto.Username
            };

            // Setup UserManager to create user successfully
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Setup UserManager to add role successfully
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Tenant"))
                .ReturnsAsync(IdentityResult.Success);

            // Setup SignInManager to sign in successfully
            _mockSignInManager.Setup(m => m.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
                .Returns(Task.CompletedTask);

            // Setup to capture the created user
            ApplicationUser capturedUser = null;
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .Callback<ApplicationUser, string>((user, password) => capturedUser = user)
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterUser(registerDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(capturedUser);
            Assert.Equal(registerDto.FullName, capturedUser.FullName);
            Assert.Equal(registerDto.IdentityCard, capturedUser.IdentityCard);
            Assert.Equal(registerDto.Email, capturedUser.Email);
            Assert.Equal(registerDto.PhoneNumber, capturedUser.PhoneNumber);
            Assert.Equal(registerDto.HomeAddress, capturedUser.HomeAddress);
            Assert.Equal(registerDto.Username, capturedUser.UserName);

            // Verify expected method calls
            _mockUserManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password), Times.Once);
            _mockUserManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Tenant"), Times.Once);
            _mockSignInManager.Verify(m => m.SignInAsync(It.IsAny<ApplicationUser>(), false, null), Times.Once);
        }

        [Fact]
        public async Task RegisterUser_WithFailedCreation_ReturnsNull()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                FullName = "John Doe",
                IdentityCard = "123456789012",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                HomeAddress = "123 Main St",
                Username = "johndoe",
                Password = "Password123!"
            };

            // Setup UserManager to fail creating user
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed" }));

            // Act
            var result = await _authService.RegisterUser(registerDto);

            // Assert
            Assert.Null(result);

            // Verify expected method calls
            _mockUserManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password), Times.Once);
            _mockUserManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            _mockSignInManager.Verify(m => m.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LoginUser_WithValidEmail_ReturnsLoginResponse()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                EmailorUsername = "john@example.com",
                Password = "Password123!"
            };

            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "johndoe",
                Email = "john@example.com",
                FullName = "John Doe",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            string jwtToken = "jwt-token-123";

            // Setup UserManager to find user by email
            _mockUserManager.Setup(m => m.FindByEmailAsync(loginDto.EmailorUsername))
                .ReturnsAsync(user);

            // Setup UserManager to check password successfully
            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(true);

            // Setup JwtService to generate token
            _mockJwtService.Setup(j => j.GenerateSecurityToken(user))
                .Returns(jwtToken);

            // Act
            var result = await _authService.LoginUser(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(jwtToken, result.Token);
            Assert.Equal(user.FullName, result.FullName);
            Assert.Equal(user.AvatarUrl, result.AvatarUrl);

            // Verify method calls
            _mockUserManager.Verify(m => m.FindByEmailAsync(loginDto.EmailorUsername), Times.Once);
            _mockUserManager.Verify(m => m.CheckPasswordAsync(user, loginDto.Password), Times.Once);
            _mockJwtService.Verify(j => j.GenerateSecurityToken(user), Times.Once);
        }

        [Fact]
        public async Task LoginUser_WithValidUsername_ReturnsLoginResponse()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                EmailorUsername = "tien.123", // Username instead of email
                Password = "Tien@123"
            };

            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "johndoe",
                Email = "john@example.com",
                FullName = "John Doe",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            string jwtToken = "jwt-token-123";

            // Setup UserManager to find user by username (email lookup will return null)
            _mockUserManager.Setup(m => m.FindByEmailAsync(loginDto.EmailorUsername))
                .ReturnsAsync((ApplicationUser)null);

            _mockUserManager.Setup(m => m.FindByNameAsync(loginDto.EmailorUsername))
                .ReturnsAsync(user);

            // Setup UserManager to check password successfully
            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(true);

            // Setup JwtService to generate token
            _mockJwtService.Setup(j => j.GenerateSecurityToken(user))
                .Returns(jwtToken);

            // Act
            var result = await _authService.LoginUser(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(jwtToken, result.Token);
            Assert.Equal(user.FullName, result.FullName);
            Assert.Equal(user.AvatarUrl, result.AvatarUrl);

            // Verify method calls
            _mockUserManager.Verify(m => m.FindByEmailAsync(loginDto.EmailorUsername), Times.Once);
            _mockUserManager.Verify(m => m.FindByNameAsync(loginDto.EmailorUsername), Times.Once);
            _mockUserManager.Verify(m => m.CheckPasswordAsync(user, loginDto.Password), Times.Once);
            _mockJwtService.Verify(j => j.GenerateSecurityToken(user), Times.Once);
        }

        [Fact]
        public async Task LoginUser_WithInvalidCredentials_ReturnsNull()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                EmailorUsername = "john@example.com",
                Password = "WrongPassword"
            };

            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "johndoe",
                Email = "john@example.com",
                FullName = "John Doe"
            };

            // Setup UserManager to find user by email
            _mockUserManager.Setup(m => m.FindByEmailAsync(loginDto.EmailorUsername))
                .ReturnsAsync(user);

            // Setup UserManager to check password (fails)
            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.LoginUser(loginDto);

            // Assert
            Assert.Null(result);

            // Verify method calls
            _mockUserManager.Verify(m => m.FindByEmailAsync(loginDto.EmailorUsername), Times.Once);
            _mockUserManager.Verify(m => m.CheckPasswordAsync(user, loginDto.Password), Times.Once);
            _mockJwtService.Verify(j => j.GenerateSecurityToken(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task LoginUser_WithNonExistentUser_ReturnsNull()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                EmailorUsername = "nonexistent@example.com",
                Password = "Password123!"
            };

            // Setup UserManager to not find the user
            _mockUserManager.Setup(m => m.FindByEmailAsync(loginDto.EmailorUsername))
                .ReturnsAsync((ApplicationUser)null);

            _mockUserManager.Setup(m => m.FindByNameAsync(loginDto.EmailorUsername))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _authService.LoginUser(loginDto);

            // Assert
            Assert.Null(result);

            // Verify method calls
            _mockUserManager.Verify(m => m.FindByEmailAsync(loginDto.EmailorUsername), Times.Once);
            _mockUserManager.Verify(m => m.FindByNameAsync(loginDto.EmailorUsername), Times.Once);
            _mockUserManager.Verify(m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            _mockJwtService.Verify(j => j.GenerateSecurityToken(It.IsAny<ApplicationUser>()), Times.Never);
        }
    }
}