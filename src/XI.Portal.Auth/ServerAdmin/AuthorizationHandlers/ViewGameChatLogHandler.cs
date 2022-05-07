using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.ServerAdmin.AuthorizationRequirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Auth.ServerAdmin.AuthorizationHandlers
{
    public class ViewGameChatLogHandler : AuthorizationHandler<ViewGameChatLog, GameType>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ViewGameChatLog requirement, GameType resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.ToString()))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, resource.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}