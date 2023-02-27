using System.Security.Claims;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.AdminWebApp.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static string Username(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static string Email(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static string XtremeIdiotsId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirst(UserProfileClaimType.XtremeIdiotsId)?.Value;
        }

        public static string UserProfileId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirst(UserProfileClaimType.UserProfileId)?.Value;
        }

        public static string PhotoUrl(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirst(UserProfileClaimType.PhotoUrl)?.Value;
        }

        public static Tuple<GameType[], Guid[]> ClaimedGamesAndItems(this ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
        {
            var gameTypes = new List<GameType>();
            var servers = new List<Guid>();

            if (claimsPrincipal.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                gameTypes = Enum.GetValues(typeof(GameType)).Cast<GameType>().ToList();

            var claims = claimsPrincipal.Claims.Where(claim => requiredClaims.Contains(claim.Type));

            foreach (var claim in claims)
            {
                if (Enum.TryParse(claim.Type, out GameType gameType))
                    gameTypes.Add(Enum.Parse<GameType>(claim.Value));

                if (Guid.TryParse(claim.Value, out var guid))
                    servers.Add(guid);
            }

            gameTypes = gameTypes.Distinct().OrderBy(g => g).ToList();
            return new Tuple<GameType[], Guid[]>(gameTypes.ToArray(), servers.ToArray());
        }

        public static List<GameType> ClaimedGameTypes(this ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
        {
            var gameTypes = new List<GameType>();

            if (claimsPrincipal.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                gameTypes = Enum.GetValues(typeof(GameType)).Cast<GameType>().ToList();

            var claims = claimsPrincipal.Claims.Where(claim => requiredClaims.Contains(claim.Type));

            foreach (var claim in claims)
                if (Enum.TryParse(claim.Value, out GameType gameType))
                    gameTypes.Add(gameType);

            return gameTypes.Distinct().OrderBy(g => g).ToList();
        }

        public static List<GameType> GetGameTypesForGameServers(this ClaimsPrincipal claimsPrincipal)
        {
            var requiredClaims = new[]
            {
                UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin
            };

            return claimsPrincipal.ClaimedGameTypes(requiredClaims);
        }
    }
}