using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.ServerAdmin.AuthorizationRequirements;

namespace XI.Portal.Auth.ServerAdmin.AuthorizationHandlers
{
    public class ViewGameChatLogHandler : AuthorizationHandler<ViewGameChatLog, string>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ViewGameChatLog requirement, string resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, resource))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}