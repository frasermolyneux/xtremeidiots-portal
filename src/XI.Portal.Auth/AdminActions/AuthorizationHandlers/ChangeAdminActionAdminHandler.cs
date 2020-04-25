using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.AdminActions.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Players.Dto;

namespace XI.Portal.Auth.AdminActions.AuthorizationHandlers
{
    public class ChangeAdminActionAdminHandler : AuthorizationHandler<ChangeAdminActionAdmin, AdminActionDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ChangeAdminActionAdmin requirement, AdminActionDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.GameType.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}