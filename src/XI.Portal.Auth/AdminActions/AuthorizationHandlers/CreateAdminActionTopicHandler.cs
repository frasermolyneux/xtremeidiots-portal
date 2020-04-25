using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.AdminActions.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Players.Dto;

namespace XI.Portal.Auth.AdminActions.AuthorizationHandlers
{
    public class CreateAdminActionTopicHandler : AuthorizationHandler<CreateAdminActionTopic, AdminActionDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CreateAdminActionTopic requirement, AdminActionDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.GameType.ToString()))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, resource.GameType.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}