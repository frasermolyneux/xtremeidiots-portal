using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Web.Auth.Constants;
using XI.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Web.Auth.Handlers
{
    public class UsersAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessUsers)
                    HandleAccessUsers(context, requirement);

                if (requirement is CreateUserClaim)
                    HandleCreateUserClaim(context, requirement);

                if (requirement is DeleteUserClaim)
                    HandleDeleteUserClaim(context, requirement);
            }

            return Task.CompletedTask;
        }

        private void HandleDeleteUserClaim(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                var gameType = (GameType)context.Resource;

                if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin && claim.Value == gameType.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleCreateUserClaim(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                var gameType = (GameType)context.Resource;

                if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin && claim.Value == gameType.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleAccessUsers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);
        }
    }
}
