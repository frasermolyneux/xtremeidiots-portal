using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Extensions
{
    public static class RconMonitorDbSetAuthExtensions
    {
        public static IQueryable<RconMonitors> ApplyAuthPolicies(this DbSet<RconMonitors> rconMonitors, ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
        {
            if (claimsPrincipal == null || requiredClaims == null)
                return rconMonitors.AsQueryable();

            var (gameTypes, serverIds) = claimsPrincipal.ClaimedGamesAndServers(requiredClaims);
            var query = rconMonitors.Include(monitor => monitor.GameServerServer).AsQueryable();

            return query.Where(server => gameTypes.Contains(server.GameServerServer.GameType)).AsQueryable();
        }
    }
}