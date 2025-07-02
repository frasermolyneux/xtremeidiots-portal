using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    public class PlayersAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessPlayers)
                    HandleAccessPlayers(context, requirement);

                if (requirement is DeletePlayer)
                    HandleDeletePlayer(context, requirement);

                if (requirement is ViewPlayers)
                    HandleViewPlayers(context, requirement);
            }

            return Task.CompletedTask;
        }

        private void HandleViewPlayers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.GameAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.Moderator, context.Resource.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleDeletePlayer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);
        }

        private void HandleAccessPlayers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.GameAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.Moderator))
                context.Succeed(requirement);
        }
    }
}
