using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Players.Dto;

namespace XI.Portal.Auth.AuthorizationHandlers
{
    public class DeleteAdminActionHandler : AuthorizationHandler<DeleteAdminAction, AdminActionDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DeleteAdminAction requirement, AdminActionDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}