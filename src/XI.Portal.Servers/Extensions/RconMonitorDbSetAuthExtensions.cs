using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Auth.Extensions;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Extensions
{
    public static class RconMonitorDbSetAuthExtensions
    {
        public static IQueryable<RconMonitors> ApplyAuthPolicies(this DbSet<RconMonitors> banFileMonitors, ClaimsPrincipal claimsPrincipal)
        {
            var gameTypes = claimsPrincipal.ClaimedGameTypes();
            var query = banFileMonitors.Include(monitor => monitor.GameServerServer).AsQueryable();

            return query.Where(server => gameTypes.Contains(server.GameServerServer.GameType)).AsQueryable();
        }
    }
}