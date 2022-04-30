using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Players.AuthorizationRequirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

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