using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Maps.AuthorizationRequirements;

namespace XI.Portal.Auth.Maps.AuthorizationHandlers
{
    public class AccessMapsHandler : AuthorizationHandler<AccessMaps>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessMaps requirement)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}