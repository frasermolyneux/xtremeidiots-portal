using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using XI.Portal.Auth.Contract.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Web.Extensions
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

        public static Tuple<string[], Guid[]> ClaimedGamesAndItems(this ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
        {
            var gameTypes = new List<string>();
            var servers = new List<Guid>();

            if (claimsPrincipal.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                gameTypes = Enum.GetValues(typeof(GameType)).Cast<GameType>().ToList().Select(gt => gt.ToString()).ToList();

            var claims = claimsPrincipal.Claims.Where(claim => requiredClaims.Contains(claim.Type));

            foreach (var claim in claims)
            {
                gameTypes.Add(claim.Value);

                if (Guid.TryParse(claim.Value, out var guid)) servers.Add(guid);
            }

            gameTypes = gameTypes.Distinct().OrderBy(g => g).ToList();
            return new Tuple<string[], Guid[]>(gameTypes.ToArray(), servers.ToArray());
        }

        public static List<string> ClaimedGameTypes(this ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
        {
            var gameTypes = new List<string>();

            if (claimsPrincipal.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                gameTypes = Enum.GetValues(typeof(GameType)).Cast<GameType>().ToList().Select(gt => gt.ToString()).ToList();

            var claims = claimsPrincipal.Claims.Where(claim => requiredClaims.Contains(claim.Type));

            foreach (var claim in claims)
                gameTypes.Add(claim.Value);

            return gameTypes.Distinct().OrderBy(g => g).ToList();
        }

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