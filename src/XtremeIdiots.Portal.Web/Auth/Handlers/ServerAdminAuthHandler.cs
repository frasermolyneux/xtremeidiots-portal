using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    public class ServerAdminAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessLiveRcon)
                    HandleAccessLiveRcon(context, requirement);

                if (requirement is AccessServerAdmin)
                    HandleAccessServerAdmin(context, requirement);

                if (requirement is ViewGameChatLog)
                    HandleViewGameChatLog(context, requirement);

                if (requirement is ViewGlobalChatLog)
                    HandleViewGlobalChatLog(context, requirement);

                if (requirement is ViewLiveRcon)
                    HandleViewLiveRcon(context, requirement);

                if (requirement is ViewServerChatLog)
                    HandleViewServerChatLog(context, requirement);

                if (requirement is ManageMaps)
                    HandleManageMaps(context, requirement);

                if (requirement is LockChatMessages)
                    HandleLockChatMessages(context, requirement);
            }

            return Task.CompletedTask;
        }

        private void HandleViewServerChatLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                var gameType = (GameType)context.Resource;

                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.GameAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.Moderator, gameType.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleViewLiveRcon(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                var gameType = (GameType)context.Resource;

                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.GameAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.LiveRcon, gameType.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleViewGlobalChatLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.GameAdmin))
                context.Succeed(requirement);
        }

        private void HandleViewGameChatLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                var gameType = (GameType)context.Resource;

                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.GameAdmin, gameType.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleAccessServerAdmin(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.GameAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.Moderator))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.ServerAdmin))
                context.Succeed(requirement);
        }

        private void HandleAccessLiveRcon(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.GameAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.LiveRcon))
                context.Succeed(requirement);
        }

        private static void HandleManageMaps(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            var requiredClaims = new string[] { UserProfileClaimType.HeadAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);
        }

        private void HandleLockChatMessages(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.GameAdmin))
                context.Succeed(requirement);

            // Only allow moderators to lock chat messages if it's for a specific game
            if (context.Resource is GameType)
            {
                var gameType = (GameType)context.Resource;

                if (context.User.HasClaim(UserProfileClaimType.Moderator, gameType.ToString()))
                    context.Succeed(requirement);
            }
        }
    }
}
