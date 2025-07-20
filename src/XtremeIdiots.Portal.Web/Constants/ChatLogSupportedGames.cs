using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Constants;

/// <summary>
/// Defines which game types support chat log functionality
/// </summary>
/// <remarks>
/// Currently supports Call of Duty 2, 4, and 5 (World at War) which have 
/// compatible chat log formats and server communication protocols
/// </remarks>
public static class ChatLogSupportedGames
{
    /// <summary>
    /// Gets an enumerable of game types that support chat log tracking
    /// </summary>
    /// <returns>Supported game types for chat log functionality</returns>
    public static IEnumerable<GameType> Games => SupportedGameTypes;

    /// <summary>
    /// Gets a read-only list of game types that support chat log functionality
    /// </summary>
    public static IReadOnlyList<GameType> SupportedGameTypes { get; } =
    [
        GameType.CallOfDuty2,
        GameType.CallOfDuty4,
        GameType.CallOfDuty5
    ];

    /// <summary>
    /// Determines if the specified game type supports chat log functionality
    /// </summary>
    /// <param name="gameType">The game type to check</param>
    /// <returns>True if chat logs are supported for this game type</returns>
    public static bool IsSupported(GameType gameType)
    {
        return SupportedGameTypes.Contains(gameType);
    }
}