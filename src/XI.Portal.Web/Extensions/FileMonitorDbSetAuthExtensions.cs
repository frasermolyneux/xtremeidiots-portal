using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Web.Extensions
{
    public static class FileMonitorDbSetAuthExtensions
    {
        public static IQueryable<FileMonitors> ApplyAuthPolicies(this DbSet<FileMonitors> fileMonitors, ClaimsPrincipal claimsPrincipal)
        {
            var gameTypes = claimsPrincipal.ClaimedGameTypes();
            var query = fileMonitors.Include(monitor => monitor.GameServerServer).AsQueryable();

            return query.Where(server => gameTypes.Contains(server.GameServerServer.GameType)).AsQueryable();
        }
    }
}