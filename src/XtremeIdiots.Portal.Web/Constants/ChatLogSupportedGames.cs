using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Constants;

public static class ChatLogSupportedGames
{

    public static IEnumerable<GameType> Games {
        get {
            yield return GameType.CallOfDuty2;
            yield return GameType.CallOfDuty4;
            yield return GameType.CallOfDuty5;
        }
    }

    public static IReadOnlyList<GameType> SupportedGameTypes { get; } =
    [
        GameType.CallOfDuty2,
        GameType.CallOfDuty4,
        GameType.CallOfDuty5
    ];

    public static bool IsSupported(GameType gameType)
    {
        return SupportedGameTypes.Contains(gameType);
    }
}