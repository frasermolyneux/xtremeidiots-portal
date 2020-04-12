using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Extensions
{
    public static class GameServerDbSetAuthExtensions
    {
        public static IQueryable<GameServers> ApplyAuthPolicies(this DbSet<GameServers> gameServers, ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
        {
            var (gameTypes, serverIds) = claimsPrincipal.ClaimedGamesAndServers(requiredClaims);
            return gameServers.Where(server => gameTypes.Contains(server.GameType) || serverIds.Contains(server.ServerId)).AsQueryable();
        }
    }
}