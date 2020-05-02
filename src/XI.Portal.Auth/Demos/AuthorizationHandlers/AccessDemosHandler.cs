using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using XI.Portal.Auth.Demos.AuthorizationRequirements;

namespace XI.Portal.Auth.Demos.AuthorizationHandlers
{
    public class AccessDemosHandler : AuthorizationHandler<AccessDemos>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessDemosHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessDemos requirement)
        {
            if (context.User.Identity.IsAuthenticated)
                context.Succeed(requirement);

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.Request.Headers.ContainsKey("demo-manager-auth-key"))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}