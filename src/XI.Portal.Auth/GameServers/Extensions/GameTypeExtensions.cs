using System.Collections.Generic;
using System.Security.Claims;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;

namespace XI.Portal.Auth.GameServers.Extensions
{
    public static class GameTypeExtensions
    {
        public static List<string> GetGameTypesForGameServers(this ClaimsPrincipal claimsPrincipal)
        {
            var requiredClaims = new[]
            {
                XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin
            };

            return claimsPrincipal.ClaimedGameTypes(requiredClaims);
        }
    }
}