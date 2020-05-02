using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Players.AuthorizationRequirements;
using XI.Portal.Players.Dto;

namespace XI.Portal.Auth.Players.AuthorizationHandlers
{
    public class DeletePlayerHandler : AuthorizationHandler<DeletePlayer, PlayerDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DeletePlayer requirement, PlayerDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}