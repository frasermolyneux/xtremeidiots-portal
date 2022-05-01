using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using XI.Portal.Auth.AdminActions.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XI.Portal.Auth.AdminActions.AuthorizationHandlers
{
    public class EditAdminActionHandler : AuthorizationHandler<EditAdminAction, AdminActionDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EditAdminAction requirement, AdminActionDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.GameType))
                context.Succeed(requirement);

            switch (resource.Type)
            {
                case "Observation":
                    if ((context.User.HasClaim(XtremeIdiotsClaimTypes.Moderator, resource.GameType) ||
                         context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, resource.GameType)) &&
                        context.User.XtremeIdiotsId() == resource.AdminId)
                        context.Succeed(requirement);
                    break;
                case "Warning":
                    if ((context.User.HasClaim(XtremeIdiotsClaimTypes.Moderator, resource.GameType) ||
                         context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, resource.GameType)) &&
                        context.User.XtremeIdiotsId() == resource.AdminId)
                        context.Succeed(requirement);
                    break;
                case "Kick":
                    if ((context.User.HasClaim(XtremeIdiotsClaimTypes.Moderator, resource.GameType) ||
                         context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, resource.GameType)) &&
                        context.User.XtremeIdiotsId() == resource.AdminId)
                        context.Succeed(requirement);
                    break;
                case "TempBan":
                    if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, resource.GameType) &&
                        context.User.XtremeIdiotsId() == resource.AdminId)
                        context.Succeed(requirement);
                    break;
                case "Ban":
                    if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, resource.GameType) &&
                        context.User.XtremeIdiotsId() == resource.AdminId)
                        context.Succeed(requirement);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}