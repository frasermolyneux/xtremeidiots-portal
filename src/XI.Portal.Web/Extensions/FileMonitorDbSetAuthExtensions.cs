using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy.CommonTypes;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Web.Constants;

namespace XI.Portal.Web.Extensions
{
    public static class FileMonitorDbSetAuthExtensions
    {
        public static IQueryable<FileMonitors> ApplyAuthPolicies(this DbSet<FileMonitors> fileMonitors, ClaimsPrincipal claimsPrincipal)
        {
            var gameClaims = claimsPrincipal.Claims.Where(claim => claim.Type == XtremeIdiotsClaimTypes.Game);
            var gameTitles = gameClaims.Select(claim => claim.Value).ToList();

            var gameTypes = gameTitles.Select(Enum.Parse<GameType>);

            var query = fileMonitors.Include(monitor => monitor.GameServerServer).AsQueryable();

            return query.Where(server => gameTypes.Contains(server.GameServerServer.GameType)).AsQueryable();
        }
    }
}