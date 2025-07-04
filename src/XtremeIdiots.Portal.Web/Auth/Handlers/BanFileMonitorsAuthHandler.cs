﻿using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
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
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.BanFileMonitor))
                context.Succeed(requirement);
        }

        private void HandleCreateBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, gameServerId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.BanFileMonitor, gameServerId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleDeleteBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, gameServerId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.BanFileMonitor, gameServerId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleEditBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, gameServerId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.BanFileMonitor, gameServerId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleViewBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, gameServerId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.BanFileMonitor, gameServerId.ToString()))
                    context.Succeed(requirement);
            }
        }
    }
}
