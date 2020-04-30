using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.FileMonitors.AuthorizationRequirements;

namespace XI.Portal.Auth.FileMonitors.AuthorizationHandlers
{
    public class AccessFileMonitorsHandler : AuthorizationHandler<AccessFileMonitors>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessFileMonitors requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == PortalClaimTypes.FileMonitor))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}