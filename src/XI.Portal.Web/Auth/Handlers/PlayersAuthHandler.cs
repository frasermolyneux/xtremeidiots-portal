using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Web.Auth.Constants;
using XI.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Portal.Web.Auth.Handlers
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
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.Moderator, context.Resource.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleDeletePlayer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);
        }

        private void HandleAccessPlayers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.GameAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.Moderator))
                context.Succeed(requirement);
        }
    }
}
