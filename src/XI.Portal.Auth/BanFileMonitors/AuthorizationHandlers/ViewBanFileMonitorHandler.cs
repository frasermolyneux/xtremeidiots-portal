using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using XI.Portal.Auth.BanFileMonitors.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XI.Portal.Auth.BanFileMonitors.AuthorizationHandlers
{
    public class ViewBanFileMonitorHandler : AuthorizationHandler<ViewBanFileMonitor, BanFileMonitorDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ViewBanFileMonitor requirement, BanFileMonitorDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.GameType))
                context.Succeed(requirement);

            if (context.User.HasClaim(PortalClaimTypes.BanFileMonitor, resource.ServerId.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}