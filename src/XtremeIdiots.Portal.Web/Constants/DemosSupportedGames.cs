using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Constants;

/// <summary>
/// Defines the game types that support demo recording and playback functionality
/// </summary>
public static class DemosSupportedGames
{
    /// <summary>
    /// Gets the collection of game types that support demo functionality
    /// </summary>
    public static IEnumerable<GameType> Games => [
        GameType.CallOfDuty2,
        GameType.CallOfDuty4,
        GameType.CallOfDuty5
    ];
}