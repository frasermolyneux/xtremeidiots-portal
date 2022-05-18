using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using XI.Portal.Servers.Models;
using XI.Portal.Web.Auth.Constants;

namespace XI.Portal.Web.Extensions
{
    public static class GameServerStatusFilterModelExtensions
    {
        public static GameServerStatusFilterModel ApplyAuthForGameServerStatus(this GameServerStatusFilterModel filterModel, ClaimsPrincipal claimsPrincipal)
        {
            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, PortalClaimTypes.GameServer };
            var (gameTypes, serverIds) = claimsPrincipal.ClaimedGamesAndItems(requiredClaims);

            List<Guid> legacyGuids = new List<Guid>();
            foreach (var guid in serverIds)
            {
                legacyGuids.Add(guid);
            }

            filterModel.GameTypes = gameTypes.ToList();
            filterModel.ServerIds = legacyGuids;

            return filterModel;
        }
    }
}