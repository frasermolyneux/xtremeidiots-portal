using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy.CommonTypes;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Web.Constants;

namespace XI.Portal.Web.Extensions
{
    public static class GameServerDbSetAuthExtensions
    {
        public static IQueryable<GameServers> ApplyAuthPolicies(this DbSet<GameServers> gameServers, ClaimsPrincipal claimsPrincipal)
        {
            var gameClaims = claimsPrincipal.Claims.Where(claim => claim.Type == XtremeIdiotsClaimTypes.Game);
            var gameTitles = gameClaims.Select(claim => claim.Value).ToList();

            var gameTypes = gameTitles.Select(Enum.Parse<GameType>);

            return gameServers.Where(server => gameTypes.Contains(server.GameType)).AsQueryable();
        }
    }
}