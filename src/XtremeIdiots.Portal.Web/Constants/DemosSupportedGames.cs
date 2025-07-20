using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Constants;

public static class DemosSupportedGames
{

    public static IEnumerable<GameType> Games {
        get {
            yield return GameType.CallOfDuty2;
            yield return GameType.CallOfDuty4;
            yield return GameType.CallOfDuty5;
        }
    }
}