using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.GameServers.AuthorizationRequirements;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Auth.GameServers.AuthorizationHandlers
{
    public class EditGameServerFtpHandler : AuthorizationHandler<EditGameServerFtp, GameServerDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EditGameServerFtp requirement, GameServerDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}