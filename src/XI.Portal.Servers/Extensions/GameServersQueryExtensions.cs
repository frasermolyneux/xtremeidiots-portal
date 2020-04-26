using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Extensions
{
    public static class GameServersQueryExtensions
    {
        public static IQueryable<GameServers> ApplyAuth(this IQueryable<GameServers> gameServers, ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
        {
            if (claimsPrincipal == null || requiredClaims == null)
                return gameServers.AsQueryable();

            var (gameTypes, serverIds) = claimsPrincipal.ClaimedGamesAndItems(requiredClaims);
            return gameServers.Where(server => gameTypes.Contains(server.GameType) || serverIds.Contains(server.ServerId)).AsQueryable();
        }
    }
}