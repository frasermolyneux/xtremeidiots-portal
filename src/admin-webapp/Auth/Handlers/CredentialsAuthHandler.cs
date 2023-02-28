using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.AdminWebApp.Auth.Requirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.Handlers
{
    public class CredentialsAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
                if (requirement is AccessCredentials)
                    HandleAccessCredentials(requirement, context);

            return Task.CompletedTask;
        }

        private void HandleAccessCredentials(IAuthorizationRequirement requirement, AuthorizationHandlerContext context)
        {
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.GameAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.RconCredentials))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.FtpCredentials))
                context.Succeed(requirement);
        }
    }
}
