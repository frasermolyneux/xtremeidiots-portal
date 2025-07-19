using System.Security.Claims;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to extract XtremeIdiots Portal specific claims
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user's display username from claims
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal</param>
    /// <returns>The username, or null if not found</returns>
    public static string? Username(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Gets the user's email address from claims
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal</param>
    /// <returns>The email address, or null if not found</returns>
    public static string? Email(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets the user's XtremeIdiots forum ID from claims
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal</param>
    /// <returns>The XtremeIdiots ID, or null if not found</returns>
    public static string? XtremeIdiotsId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(UserProfileClaimType.XtremeIdiotsId)?.Value;
    }

    /// <summary>
    /// Gets the user's internal profile ID from claims
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal</param>
    /// <returns>The user profile ID, or null if not found</returns>
    public static string? UserProfileId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(UserProfileClaimType.UserProfileId)?.Value;
    }

    /// <summary>
    /// Gets the user's profile photo URL from claims
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal</param>
    /// <returns>The photo URL, or null if not found</returns>
    public static string? PhotoUrl(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(UserProfileClaimType.PhotoUrl)?.Value;
    }

    /// <summary>
    /// Gets the game types and item IDs that the user has claimed access to based on required claim types
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal</param>
    /// <param name="requiredClaims">The required claim types to check for</param>
    /// <returns>A tuple containing arrays of accessible game types and item IDs</returns>
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

    /// <summary>
    /// Gets the game types that the user has claimed access to based on required claim types
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal</param>
    /// <param name="requiredClaims">The required claim types to check for</param>
    /// <returns>A list of accessible game types</returns>
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

    /// <summary>
    /// Gets the game types that the user can manage game servers for
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal</param>
    /// <returns>A list of game types for game server management</returns>
    public static List<GameType> GetGameTypesForGameServers(this ClaimsPrincipal claimsPrincipal)
    {
        var requiredClaims = new[]
        {
            UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin
        };

        return claimsPrincipal.ClaimedGameTypes(requiredClaims);
    }
}