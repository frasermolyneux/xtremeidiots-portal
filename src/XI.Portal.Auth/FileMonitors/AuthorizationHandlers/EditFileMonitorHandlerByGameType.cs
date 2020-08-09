using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.FileMonitors.AuthorizationRequirements;

namespace XI.Portal.Auth.FileMonitors.AuthorizationHandlers
{
    public class EditFileMonitorHandlerByGameType : AuthorizationHandler<EditFileMonitor, GameType>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EditFileMonitor requirement, GameType resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.ToString()))
                context.Succeed(requirement);

            if (context.User.HasClaim(PortalClaimTypes.FileMonitor, resource.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}