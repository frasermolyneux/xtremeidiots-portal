using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Auth.Demos.AuthorizationRequirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XI.Portal.Auth.Demos.AuthorizationHandlers
{
    public class DeleteDemoHandler : AuthorizationHandler<DeleteDemo, DemoDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DeleteDemo requirement, DemoDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.Game.ToString()))
                context.Succeed(requirement);

            if (context.User.LegacyXtremeIdiotsId() == resource.UserId)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}