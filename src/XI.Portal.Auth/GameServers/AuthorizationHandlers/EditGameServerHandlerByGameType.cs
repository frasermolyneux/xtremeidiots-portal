using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.GameServers.AuthorizationRequirements;

namespace XI.Portal.Auth.GameServers.AuthorizationHandlers
{
    public class EditGameServerHandlerByGameType : AuthorizationHandler<EditGameServer, GameType>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EditGameServer requirement, GameType resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.ToString()))
                context.Succeed(requirement);

            if (context.User.HasClaim(PortalClaimTypes.GameServer, resource.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}