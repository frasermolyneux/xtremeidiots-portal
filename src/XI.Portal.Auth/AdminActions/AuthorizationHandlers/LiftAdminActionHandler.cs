using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using XI.Portal.Auth.AdminActions.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XI.Portal.Auth.AdminActions.AuthorizationHandlers
{
    public class LiftAdminActionHandler : AuthorizationHandler<LiftAdminAction, AdminActionDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, LiftAdminAction requirement, AdminActionDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.GameType))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, resource.GameType) &&
                context.User.XtremeIdiotsId() == resource.AdminId)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}