﻿using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.GameServers.AuthorizationRequirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XI.Portal.Auth.GameServers.AuthorizationHandlers
{
    public class EditGameServerHandler : AuthorizationHandler<EditGameServer, GameServerDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EditGameServer requirement, GameServerDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, resource.GameType.ToString()))
                context.Succeed(requirement);

            if (context.User.HasClaim(PortalClaimTypes.GameServer, resource.GameType.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}