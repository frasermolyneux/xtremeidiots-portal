using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Auth.Requirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.Handlers
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
                var (gameType, gameServerId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.BanFileMonitor, gameServerId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleDeleteBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, gameServerId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.BanFileMonitor, gameServerId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleEditBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, gameServerId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.BanFileMonitor, gameServerId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleViewBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, gameServerId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.BanFileMonitor, gameServerId.ToString()))
                    context.Succeed(requirement);
            }
        }
    }
}
