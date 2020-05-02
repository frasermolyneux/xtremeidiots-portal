using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.ServerAdmin.AuthorizationRequirements;

namespace XI.Portal.Auth.ServerAdmin.AuthorizationHandlers
{
    public class ViewGlobalChatLogHandler : AuthorizationHandler<ViewGlobalChatLog>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ViewGlobalChatLog requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.GameAdmin))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}