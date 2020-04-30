using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.FileMonitors.AuthorizationRequirements;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Auth.FileMonitors.AuthorizationHandlers
{
    public class ViewFileMonitorHandler : AuthorizationHandler<ViewFileMonitor, FileMonitorDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ViewFileMonitor requirement, FileMonitorDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.GameServer.GameType.ToString()))
                context.Succeed(requirement);

            if (context.User.HasClaim(PortalClaimTypes.FileMonitor, resource.GameServer.ServerId.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}