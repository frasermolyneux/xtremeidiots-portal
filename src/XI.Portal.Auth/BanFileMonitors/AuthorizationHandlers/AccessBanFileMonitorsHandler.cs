using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.BanFileMonitors.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;

namespace XI.Portal.Auth.BanFileMonitors.AuthorizationHandlers
{
    public class AccessBanFileMonitorsHandler : AuthorizationHandler<AccessBanFileMonitors>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessBanFileMonitors requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == PortalClaimTypes.BanFileMonitor))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}