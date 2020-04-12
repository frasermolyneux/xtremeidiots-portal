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

        public static Tuple<IEnumerable<GameType>, IEnumerable<Guid>> ClaimedGamesAndServers(this ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
        {
            var gameTypes = new List<GameType>();
            var servers = new List<Guid>();

            if (claimsPrincipal.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                gameTypes = Enum.GetValues(typeof(GameType)).Cast<GameType>().ToList();

            var claims = claimsPrincipal.Claims.Where(claim => requiredClaims.Contains(claim.Type));

            foreach (var claim in claims)
            {
                if (Enum.TryParse(claim.Value, out GameType gameType)) gameTypes.Add(gameType);

                if (Guid.TryParse(claim.Value, out var guid)) servers.Add(guid);
            }

            return new Tuple<IEnumerable<GameType>, IEnumerable<Guid>>(gameTypes, servers);
        }


        public static bool HasGameClaim(this ClaimsPrincipal claimsPrincipal, GameType gameType, IEnumerable<string> requiredClaims)
        {
            return claimsPrincipal.Claims.Any(claim => requiredClaims.Contains(claim.Type) && claim.Value == gameType.ToString());
        }

        public static IEnumerable<GameType> ClaimedGameTypes(this ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
        {
            var gameTypes = new List<GameType>();

            if (claimsPrincipal.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                gameTypes = Enum.GetValues(typeof(GameType)).Cast<GameType>().ToList();

            var claims = claimsPrincipal.Claims.Where(claim => requiredClaims.Contains(claim.Type));

            foreach (var claim in claims)
                if (Enum.TryParse(claim.Value, out GameType gameType))
                    gameTypes.Add(gameType);

            return gameTypes;
        }
    }
}