using System.Security.Claims;
using XI.Portal.Data.Legacy.CommonTypes;
using XI.Portal.Web.Constants;

namespace XI.Portal.Web.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static string Username(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(ClaimTypes.Name);
        }

        public static string Email(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(ClaimTypes.Email);
        }

        public static string XtremeIdiotsId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(XtremeIdiotsClaimTypes.XtremeIdiotsId);
        }

        public static string PhotoUrl(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(XtremeIdiotsClaimTypes.PhotoUrl);
        }

        public static bool HasGameTypeClaim(this ClaimsPrincipal claimsPrincipal, GameType gameType)
        {
            return claimsPrincipal.HasClaim(XtremeIdiotsClaimTypes.Game, gameType.ToString());
        }
    }
}