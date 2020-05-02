using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Home.AuthorizationRequirements;

namespace XI.Portal.Auth.Home.AuthorizationHandlers
{
    public class AccessHomeHandler : AuthorizationHandler<AccessHome>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessHome requirement)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}