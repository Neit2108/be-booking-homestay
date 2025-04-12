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
            return httpContext.User.Identity?.IsAuthenticated == true
                   && httpContext.User.IsInRole("Admin");
        }
    }
}
