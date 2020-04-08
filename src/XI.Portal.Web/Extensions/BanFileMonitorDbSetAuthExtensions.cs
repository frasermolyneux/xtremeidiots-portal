using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Web.Extensions
{
    public static class BanFileMonitorDbSetAuthExtensions
    {
        public static IQueryable<BanFileMonitors> ApplyAuthPolicies(this DbSet<BanFileMonitors> banFileMonitors, ClaimsPrincipal claimsPrincipal)
        {
            var gameTypes = claimsPrincipal.ClaimedGameTypes();
            var query = banFileMonitors.Include(monitor => monitor.GameServerServer).AsQueryable();

            return query.Where(server => gameTypes.Contains(server.GameServerServer.GameType)).AsQueryable();
        }
    }
}