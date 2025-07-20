using System.Security.Claims;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Extensions;

public static class ClaimsPrincipalExtensions
{

    public static string? Username(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
    }

    public static string? Email(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
    }

    public static string? XtremeIdiotsId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(UserProfileClaimType.XtremeIdiotsId)?.Value;
    }

    public static string? UserProfileId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(UserProfileClaimType.UserProfileId)?.Value;
    }

    public static string? PhotoUrl(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(UserProfileClaimType.PhotoUrl)?.Value;
    }

    public static (GameType[] gameTypes, Guid[] itemIds) ClaimedGamesAndItems(this ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
    {
        var gameTypes = new List<GameType>();
        var servers = new List<Guid>();

        if (claimsPrincipal.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            gameTypes = [.. Enum.GetValues<GameType>()];

        var claims = claimsPrincipal.Claims.Where(claim => requiredClaims.Contains(claim.Type));

        foreach (var claim in claims)
        {
            if (Enum.TryParse(claim.Type, out GameType gameType))
                gameTypes.Add(Enum.Parse<GameType>(claim.Value));

            if (Guid.TryParse(claim.Value, out var guid))
                servers.Add(guid);
        }

        return ([.. gameTypes.Distinct().Order()], [.. servers]);
    }

    public static List<GameType> ClaimedGameTypes(this ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
    {
        var gameTypes = new List<GameType>();

        if (claimsPrincipal.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            gameTypes = [.. Enum.GetValues<GameType>()];

        var claims = claimsPrincipal.Claims.Where(claim => requiredClaims.Contains(claim.Type));

        foreach (var claim in claims)
            if (Enum.TryParse(claim.Value, out GameType gameType))
                gameTypes.Add(gameType);

        return [.. gameTypes.Distinct().Order()];
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