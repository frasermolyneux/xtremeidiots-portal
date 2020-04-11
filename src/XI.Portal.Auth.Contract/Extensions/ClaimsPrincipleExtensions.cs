using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;

namespace XI.Portal.Auth.Contract.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static string Username(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirst(ClaimTypes.Name).Value;
        }

        public static string Email(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirst(ClaimTypes.Email).Value;
        }

        public static string XtremeIdiotsId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirst(XtremeIdiotsClaimTypes.XtremeIdiotsId).Value;
        }

        public static string PhotoUrl(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirst(XtremeIdiotsClaimTypes.PhotoUrl).Value;
        }

        public static bool HasGameTypeClaim(this ClaimsPrincipal claimsPrincipal, GameType gameType)
        {
            return claimsPrincipal.HasClaim(XtremeIdiotsClaimTypes.Game, gameType.ToString());
        }

        public static IEnumerable<GameType> ClaimedGameTypes(this ClaimsPrincipal claimsPrincipal)
        {
            var gameClaims = claimsPrincipal.Claims.Where(claim => claim.Type == XtremeIdiotsClaimTypes.Game);
            var gameTitles = gameClaims.Select(claim => claim.Value).ToList();

            var gameTypes = gameTitles.Select(title => (GameType) Enum.Parse(typeof(GameType), title)).ToList();

            return gameTypes.ToList();
        }
    }
}