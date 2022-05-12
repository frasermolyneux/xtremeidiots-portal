using System;
using System.Collections.Generic;
using System.Security.Claims;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Servers.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Auth.GameServerStatus.Extensions
{
    public static class GameServerStatusFilterModelExtensions
    {
        public static GameServerStatusFilterModel ApplyAuthForGameServerStatus(this GameServerStatusFilterModel filterModel, ClaimsPrincipal claimsPrincipal)
        {
            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, PortalClaimTypes.GameServer };
            var (gameTypes, serverIds) = claimsPrincipal.LegacyClaimedGamesAndItems(requiredClaims);

            List<GameType> legacyGameTypes = new List<GameType>();
            foreach (var gameType in gameTypes)
            {
                legacyGameTypes.Add(Enum.Parse<GameType>(gameType));
            }

            List<Guid> legacyGuids = new List<Guid>();
            foreach (var guid in serverIds)
            {
                legacyGuids.Add(guid);
            }

            filterModel.GameTypes = legacyGameTypes;
            filterModel.ServerIds = legacyGuids;

            return filterModel;
        }
    }
}