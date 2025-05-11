using Microsoft.AspNetCore.Identity;
using HomestayBookingAPI.Models;
using System;
using System.Threading.Tasks;

namespace HomestayBookingAPI.Utils
{
    public static class SeedRole
    {
        public static async Task InitializeRolesAndAdmin(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            string[] roleNames = { "Admin", "Landlord", "Tenant" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Tạo tài khoản Admin mặc định nếu chưa có
            string adminEmail = "admin@homies.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Admin",
                    HomeAddress = "Homies Stay",
                    BirthDate = DateTime.UtcNow,
                    Bio = "",
                    IdentityCard = "000000000000",
                    Favourites = null,
                    CreateAt = DateTime.UtcNow,
                };

                var result = await userManager.CreateAsync(newAdmin, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}
