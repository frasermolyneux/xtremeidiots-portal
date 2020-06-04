using System;
using System.Security.Claims;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Servers.Models;

namespace XI.Portal.Auth.GameServerStatus.Extensions
{
    public static class GameServerStatusFilterModelExtensions
    {
        public static GameServerStatusFilterModel ApplyAuthForGameServerStatus(this GameServerStatusFilterModel filterModel, ClaimsPrincipal claimsPrincipal)
        {
            var requiredClaims = new[] {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, PortalClaimTypes.GameServer};
            var (gameTypes, serverIds) = claimsPrincipal.ClaimedGamesAndItems(requiredClaims);

            filterModel.GameTypes = gameTypes;
            filterModel.ServerIds = serverIds;

            return filterModel;
        }
    }
}