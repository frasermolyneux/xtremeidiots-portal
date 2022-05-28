using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.SyncFunc.Constants
{
    public static class BanFilesSupportedGames
    {
        public static IEnumerable<GameType> Games {
            get {
                yield return GameType.CallOfDuty2;
                yield return GameType.CallOfDuty4;
                yield return GameType.CallOfDuty5;
            }
        }
    }
}