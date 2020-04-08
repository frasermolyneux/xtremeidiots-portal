using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using XI.Portal.Data.Legacy.CommonTypes;
using XI.Portal.Web.Constants;

namespace XI.Portal.Web.Extensions
{
    public static class ListClaimsGameExtensions
    {
        public static void AddGameClaimIfNotExists(this List<Claim> claims, GameType gameType)
        {
            if (!claims.Any(claim => claim.Type == XtremeIdiotsClaimTypes.Game && claim.Value == gameType.ToString())) claims.Add(new Claim(XtremeIdiotsClaimTypes.Game, gameType.ToString()));
        }
    }
}