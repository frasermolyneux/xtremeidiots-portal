using System.Security.Claims;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to extract XtremeIdiots Portal specific claims
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extracts the username from the claims principal
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal to extract from</param>
    /// <returns>The username or null if not found</returns>
    public static string? Username(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Extracts the email address from the claims principal
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal to extract from</param>
    /// <returns>The email address or null if not found</returns>
    public static string? Email(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Extracts the XtremeIdiots user ID from the claims principal
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal to extract from</param>
    /// <returns>The XtremeIdiots user ID or null if not found</returns>
    public static string? XtremeIdiotsId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(UserProfileClaimType.XtremeIdiotsId)?.Value;
    }

    /// <summary>
    /// Extracts the user profile ID from the claims principal
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal to extract from</param>
    /// <returns>The user profile ID or null if not found</returns>
    public static string? UserProfileId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(UserProfileClaimType.UserProfileId)?.Value;
    }

    /// <summary>
    /// Extracts the user's photo URL from the claims principal
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal to extract from</param>
    /// <returns>The photo URL or null if not found</returns>
    public static string? PhotoUrl(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.FindFirst(UserProfileClaimType.PhotoUrl)?.Value;
    }

    /// <summary>
    /// Extracts claimed game types and item IDs from the claims principal
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal to extract from</param>
    /// <param name="requiredClaims">The required claim types to check for</param>
    /// <returns>A tuple containing arrays of game types and item IDs the user has claims for</returns>
    public static (GameType[] gameTypes, Guid[] itemIds) ClaimedGamesAndItems(this ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
    {
        var gameTypes = new List<GameType>();
        var servers = new List<Guid>();

        if (claimsPrincipal.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            gameTypes = [.. Enum.GetValues<GameType>()];

        var claims = claimsPrincipal.Claims.Where(claim => requiredClaims.Contains(claim.Type));

        foreach (var claim in claims)
        {
            // For game-scoped claims the value holds the GameType name (e.g. COD2, COD4)
            // The previous implementation incorrectly attempted to parse claim.Type
            // (e.g. HeadAdmin, GameAdmin) as a GameType which always failed, meaning
            // HeadAdmins/GameAdmins did not get their game types populated unless they were SeniorAdmin.
            if (Enum.TryParse(claim.Value, out GameType gameTypeValue))
                gameTypes.Add(gameTypeValue);

            // For credential / server scoped claims the value is a GameServerId (GUID)
            if (Guid.TryParse(claim.Value, out var guid))
                servers.Add(guid);
        }

        return ([.. gameTypes.Distinct().Order()], [.. servers]);
    }

    /// <summary>
    /// Extracts the game types the user has claims for
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal to extract from</param>
    /// <param name="requiredClaims">The required claim types to check for</param>
    /// <returns>A list of game types the user has claims for</returns>
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
    /// Gets the game types for which the user can manage game servers
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal to extract from</param>
    /// <returns>A list of game types the user can manage servers for</returns>
    public static List<GameType> GetGameTypesForGameServers(this ClaimsPrincipal claimsPrincipal)
    {
        var requiredClaims = new[]
        {
            UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin
        };

        return claimsPrincipal.ClaimedGameTypes(requiredClaims);
    }
}