using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Constants
{
    /// <summary>
    /// Defines the collection of game types that support demo functionality.
    /// </summary>
    public static class DemosSupportedGames
    {
        /// <summary>
        /// Gets an enumerable collection of game types that support demo recording and playback.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{GameType}"/> containing all supported game types.</returns>
        public static IEnumerable<GameType> Games {
            get {
                yield return GameType.CallOfDuty2;
                yield return GameType.CallOfDuty4;
                yield return GameType.CallOfDuty5;
            }
        }
    }
}