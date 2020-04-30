using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.RconMonitors.AuthorizationRequirements;

namespace XI.Portal.Auth.RconMonitors.AuthorizationHandlers
{
    public class AccessRconMonitorsHandler : AuthorizationHandler<AccessRconMonitors>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessRconMonitors requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == PortalClaimTypes.RconMonitor))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}