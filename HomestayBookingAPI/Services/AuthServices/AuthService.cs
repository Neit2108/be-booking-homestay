using Hangfire;
using HomestayBookingAPI.DTOs;
using HomestayBookingAPI.DTOs.Password;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Services.EmailServices;
using HomestayBookingAPI.Services.JwtServices;
using HomestayBookingAPI.Services.NotifyServices;
using HomestayBookingAPI.Services.WalletServices;
using HomestayBookingAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;
using System.Text.RegularExpressions;

namespace HomestayBookingAPI.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWalletService _walletService;
        private readonly IJwtService _jwtService;
        private readonly INotifyService _notifyService;
        private readonly ILogger<AuthService> _logger;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IEmailService _emailService;
        private readonly IBackgroundJobClient _backgroundJobClient;


        public AuthService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            IWalletService walletService,
            INotifyService notifyService,
            ILogger<AuthService> logger,
            IOptions<IdentityOptions> options,
            IEmailService emailService,
            IBackgroundJobClient backgroundJobClient)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _walletService = walletService;
            _notifyService = notifyService;
            _logger = logger;
            _identityOptions = options;
            _emailService = emailService;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<ApplicationUser> RegisterUser(RegisterDTO model)
        {
            try
            {
                var user = new ApplicationUser
                {
                    FullName = model.FullName,
                    IdentityCard = model.IdentityCard,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    HomeAddress = model.HomeAddress,
                    UserName = model.Username,
                    CreateAt = DateTime.UtcNow,
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Tenant");
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    await _walletService.GetOrCreateWalletAsync(user.Id);
                    return user;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<LoginReponseDTO> LoginUser(LoginDTO model)
        {
            try
            {
                bool isEmail = Regex.IsMatch(model.EmailorUsername, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                var user = isEmail
                    ? await _userManager.FindByEmailAsync(model.EmailorUsername)
                    : await _userManager.FindByNameAsync(model.EmailorUsername);
                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    return null;
                }

                if (user.TwoFactorEnabled)
                {
                    string otp = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                    string email = TemplateMail.OTPTwoFactor(otp);
                    var jobId = _backgroundJobClient.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Xác thực đăng nhập",
                        email));

                    return new LoginReponseDTO
                    {
                        RequiresTwoFactor = true,
                        UserId = user.Id,
                    };
                }

                string token = _jwtService.GenerateSecurityToken(user);
                return new LoginReponseDTO
                {
                    Token = token,
                    FullName = user.FullName,
                    AvatarUrl = user.AvatarUrl,
                    RequiresTwoFactor = false
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<LoginReponseDTO> LoginUser2FA(string userId, string otp)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", otp);
            if (!isValid) return null;

            string token = _jwtService.GenerateSecurityToken(user);
            return new LoginReponseDTO
            {
                Token = token,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                RequiresTwoFactor = false
            };
        }


        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (request.NewPassword != request.ConfirmPassword)
            {
                return false;
            }
            if (!await _userManager.CheckPasswordAsync(user, request.OldPassword))
            {
                return false;
            }
            if (request.OldPassword == request.NewPassword)
            {
                return false;
            }
            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (result.Succeeded)
            {
                user.PasswordChangeAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                return true;
            }
            return false;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try {
                int minLength = _identityOptions.Value.Password.RequiredLength;
                _logger.LogDebug("Min length : {minLength}", minLength);
                var newPass = GeneratePassword.GenerateStrongPassword(6);
                bool isEmail = Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                var user = isEmail
                    ? await _userManager.FindByEmailAsync(email)
                    : await _userManager.FindByNameAsync(email);
                if (user == null)
                {
                    return false;
                    throw new Exception("Email không tồn tại");
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                _logger.LogDebug("Token : {token} ", token);
                _logger.LogDebug("Pass : {password}", newPass);

                var result = await _userManager.ResetPasswordAsync(user, token, newPass);
                if (result.Succeeded)
                {
                    if (isEmail)
                    {
                        await _notifyService.CreateForgotPasswordNotificationAsync(email, newPass);
                    }
                    else
                    {
                        await _notifyService.CreateForgotPasswordNotificationAsync(user.Email, newPass);
                    }
                    return true;

                }
                else
                {
                    _logger.LogDebug("Không gửi được nhé hhiihihi");
                    throw new Exception("Gửi mã xác thực không thành công");
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi mật khẩu");
                return false;
            }

        }

        public async Task<bool> SendTwoFactorEnableOtpAsync(ApplicationUser user)
        {
            if (user == null) return false;

            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            if (string.IsNullOrEmpty(token)) return false;

            var email = TemplateMail.OTPEnableTwoFactor(token);

            var jobId = _backgroundJobClient.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Mã xác thực bật bảo mật 2 lớp",
            email));

            return true;
        }

        public async Task<bool> EnableTwoFactorAsync(ApplicationUser user, string token)
        {
            if (user == null || string.IsNullOrEmpty(token)) return false;

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", token);
            if (!isValid) return false;

            var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
            return result.Succeeded;
        }

        public async Task<bool> DisableTwoFactorAsync(ApplicationUser user)
        {
            if (user == null) return false;

            var is2FA = await _userManager.GetTwoFactorEnabledAsync(user);

            if (!is2FA) return true;

            var res = await _userManager.SetTwoFactorEnabledAsync(user, false);
            return res.Succeeded;
        }

    }
}
