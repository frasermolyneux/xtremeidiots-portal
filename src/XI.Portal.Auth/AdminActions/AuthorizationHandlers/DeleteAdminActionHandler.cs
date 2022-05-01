using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using XI.Portal.Auth.AdminActions.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XI.Portal.Auth.AdminActions.AuthorizationHandlers
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