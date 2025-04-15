using Hangfire.Annotations;
using Hangfire.Dashboard;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HomestayBookingAPI.Utils
{
    public class AuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;
            var isAdmin = httpContext.User.IsInRole("Admin");
            Console.WriteLine($"IsAuthenticated: {isAuthenticated}");
            Console.WriteLine($"IsAdmin: {isAdmin}");
            if (!isAuthenticated)
            {
                var authHeader = httpContext.Request.Headers["Authorization"].ToString();
                Console.WriteLine($"Authorization Header: {authHeader}");
                Console.WriteLine("Authentication failed. Claims: " + string.Join(", ", httpContext.User.Claims.Select(c => $"{c.Type}: {c.Value}")));
                Console.WriteLine("Token validation error: Check Issuer, Audience, or Secret Key.");
            }
            return isAuthenticated && isAdmin;
        }
    }
}
