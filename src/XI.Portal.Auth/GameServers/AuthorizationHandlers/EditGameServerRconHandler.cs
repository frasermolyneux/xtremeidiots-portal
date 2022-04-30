using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.GameServers.AuthorizationRequirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XI.Portal.Auth.GameServers.AuthorizationHandlers
{
    public class EditGameServerRconHandler : AuthorizationHandler<EditGameServerRcon, GameServerDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EditGameServerRcon requirement, GameServerDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}