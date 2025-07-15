using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Constants
{
    /// <summary>
    /// Defines the game types that support chat log functionality.
    /// </summary>
    public static class ChatLogSupportedGames
    {
        /// <summary>
        /// Gets the collection of game types that support chat logs.
        /// </summary>
        /// <value>
        /// An <see cref="IEnumerable{GameType}"/> containing the supported game types.
        /// </value>
        public static IEnumerable<GameType> Games {
            get {
                yield return GameType.CallOfDuty2;
                yield return GameType.CallOfDuty4;
                yield return GameType.CallOfDuty5;
            }
        }

        /// <summary>
        /// Gets a readonly list of game types that support chat logs for improved performance.
        /// </summary>
        /// <value>
        /// A readonly <see cref="IReadOnlyList{GameType}"/> containing the supported game types.
        /// </value>
        public static IReadOnlyList<GameType> SupportedGameTypes { get; } = new[]
        {
            GameType.CallOfDuty2,
            GameType.CallOfDuty4,
            GameType.CallOfDuty5
        };

        /// <summary>
        /// Determines whether the specified game type supports chat logs.
        /// </summary>
        /// <param name="gameType">The game type to check.</param>
        /// <returns>
        /// <c>true</c> if the specified game type supports chat logs; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSupported(GameType gameType)
        {
            return SupportedGameTypes.Contains(gameType);
        }
    }
}
