using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Extensions
{
    public static class FileMonitorDbSetAuthExtensions
    {
        public static IQueryable<FileMonitors> ApplyAuthPolicies(this DbSet<FileMonitors> fileMonitors, ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
        {
            if (claimsPrincipal == null || requiredClaims == null)
                return fileMonitors.AsQueryable();

            var (gameTypes, serverIds) = claimsPrincipal.ClaimedGamesAndServers(requiredClaims);
            var query = fileMonitors.Include(monitor => monitor.GameServerServer).AsQueryable();

            return query.Where(server => gameTypes.Contains(server.GameServerServer.GameType)).AsQueryable();
        }
    }
}