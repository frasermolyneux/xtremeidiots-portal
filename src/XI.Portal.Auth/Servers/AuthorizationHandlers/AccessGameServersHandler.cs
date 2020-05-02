using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Servers.AuthorizationRequirements;

namespace XI.Portal.Auth.Servers.AuthorizationHandlers
{
    public class AccessServersHandler : AuthorizationHandler<AccessServers>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessServers requirement)
        {
            if (context.User.Identity.IsAuthenticated)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}