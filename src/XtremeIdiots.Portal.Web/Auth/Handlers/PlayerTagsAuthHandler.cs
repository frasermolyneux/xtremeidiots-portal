using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    public class PlayerTagsAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessPlayerTags)
                    HandleAccessPlayerTags(context, requirement);

                if (requirement is CreatePlayerTag)
                    HandleCreatePlayerTag(context, requirement);

                if (requirement is EditPlayerTag)
                    HandleEditPlayerTag(context, requirement);

                if (requirement is DeletePlayerTag)
                    HandleDeletePlayerTag(context, requirement);
            }

            return Task.CompletedTask;
        }

        private void HandleAccessPlayerTags(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);
        }

        private void HandleCreatePlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            // Allow senior admins, head admins, and game admins to create tags
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin) ||
                context.User.HasClaim(claim => claim.Type == UserProfileClaimType.HeadAdmin) ||
                context.User.HasClaim(claim => claim.Type == UserProfileClaimType.GameAdmin))
            {
                context.Succeed(requirement);
            }
        }

        private void HandleEditPlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);
        }

        private void HandleDeletePlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            // Senior admins can delete any tag
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            // Head admins and game admins can delete UserDefined tags for their game type
            // Note: The actual filtering by game type and UserDefined will happen in the controller
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.HeadAdmin) ||
                context.User.HasClaim(claim => claim.Type == UserProfileClaimType.GameAdmin))
            {
                context.Succeed(requirement);
            }
        }
    }
}
