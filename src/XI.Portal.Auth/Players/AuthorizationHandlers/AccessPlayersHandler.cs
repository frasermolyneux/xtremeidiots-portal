using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Players.AuthorizationRequirements;

namespace XI.Portal.Auth.Players.AuthorizationHandlers
{
    public class AccessPlayersHandler : AuthorizationHandler<AccessPlayers>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessPlayers requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.GameAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.Moderator))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}