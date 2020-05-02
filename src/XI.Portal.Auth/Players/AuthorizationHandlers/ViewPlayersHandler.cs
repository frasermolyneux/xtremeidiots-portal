using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Players.AuthorizationRequirements;

namespace XI.Portal.Auth.Players.AuthorizationHandlers
{
    public class ViewPlayersHandler : AuthorizationHandler<ViewPlayers, GameType>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ViewPlayers requirement, GameType resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.ToString()))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, resource.ToString()))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.Moderator, resource.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}