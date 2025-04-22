using HomestayBookingAPI.Data;
using HomestayBookingAPI.DTOs.User;
using HomestayBookingAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HomestayBookingAPI.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserService> _logger;
        private readonly ApplicationDbContext _context;

        public UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger, ApplicationDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        public async Task<ApplicationUser> GetUserByID(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user;
        }

        public async Task<string> GetUserRoleAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                return roles.FirstOrDefault();
            }
            return null;
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users
                                .ToListAsync();
                var res = new List<UserResponse>();

                foreach (var u in users)
                {
                    var roles = await _userManager.GetRolesAsync(u);
                    res.Add(new UserResponse
                    {
                        FullName = u.FullName,
                        IdentityCard = u.IdentityCard,
                        HomeAddress = u.HomeAddress,
                        Email = u.Email,
                        UserName = u.UserName,
                        Role = roles.ToList()
                    });
                }
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi lấy user ", ex.Message);
            }
            return new List<UserResponse>();
        }
    }
}
