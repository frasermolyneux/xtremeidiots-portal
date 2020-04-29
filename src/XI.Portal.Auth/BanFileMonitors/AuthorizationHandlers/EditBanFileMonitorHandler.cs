using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.BanFileMonitors.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Auth.BanFileMonitors.AuthorizationHandlers
{
    public class EditBanFileMonitorHandler : AuthorizationHandler<EditBanFileMonitor, BanFileMonitorDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EditBanFileMonitor requirement, BanFileMonitorDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.GameServer.GameType.ToString()))
                context.Succeed(requirement);

            if (context.User.HasClaim(PortalClaimTypes.BanFileMonitor, resource.GameServer.ServerId.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}