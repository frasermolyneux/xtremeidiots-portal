using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.GameServers.AuthorizationRequirements;

namespace XI.Portal.Auth.GameServers.AuthorizationHandlers
{
    public class AccessGameServersHandler : AuthorizationHandler<AccessGameServers>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessGameServers requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == PortalClaimTypes.GameServer))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}