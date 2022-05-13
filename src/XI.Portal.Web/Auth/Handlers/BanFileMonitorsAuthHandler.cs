using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Web.Auth.Constants;
using XI.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Web.Auth.Handlers
{
    public class BanFileMonitorsAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is ViewBanFileMonitor)
                    HandleViewBanFileMonitor(context, requirement);

                if (requirement is EditBanFileMonitor)
                    HandleEditBanFileMonitor(context, requirement);

                if (requirement is DeleteBanFileMonitor)
                    HandleDeleteBanFileMonitor(context, requirement);

                if (requirement is CreateBanFileMonitor)
                    HandleCreateBanFileMonitor(context, requirement);

                if (requirement is AccessBanFileMonitors)
                    HandleAccessBanFileMonitors(context, requirement);
            }

            return Task.CompletedTask;
        }

        private void HandleAccessBanFileMonitors(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == PortalClaimTypes.BanFileMonitor))
                context.Succeed(requirement);
        }

        private void HandleCreateBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, serverId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.BanFileMonitor, serverId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleDeleteBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, serverId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.BanFileMonitor, serverId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleEditBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, serverId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.BanFileMonitor, serverId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleViewBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, serverId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.BanFileMonitor, serverId.ToString()))
                    context.Succeed(requirement);
            }
        }
    }
}
