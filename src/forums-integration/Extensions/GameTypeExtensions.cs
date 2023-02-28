using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.ForumsIntegration.Extensions
{
    public static class GameTypeExtensions
    {
        public static int ForumIdForObservations(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.CallOfDuty2:
                    return 58;
                case GameType.CallOfDuty4:
                    return 59;
                case GameType.CallOfDuty5:
                    return 60;
                case GameType.Insurgency:
                    return 264;
                case GameType.Minecraft:
                    return 265;
                case GameType.Rust:
                    return 256;
                case GameType.Arma:
                case GameType.Arma2:
                case GameType.Arma3:
                    return 252;
                default:
                    return 28;
            }
        }

        public static int ForumIdForKicks(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.CallOfDuty2:
                    return 58;
                case GameType.CallOfDuty4:
                    return 59;
                case GameType.CallOfDuty5:
                    return 60;
                case GameType.Insurgency:
                    return 264;
                case GameType.Minecraft:
                    return 265;
                case GameType.Rust:
                    return 256;
                case GameType.Arma:
                case GameType.Arma2:
                case GameType.Arma3:
                    return 252;
                default:
                    return 28;
            }
        }

        public static int ForumIdForWarnings(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.CallOfDuty2:
                    return 58;
                case GameType.CallOfDuty4:
                    return 59;
                case GameType.CallOfDuty5:
                    return 60;
                case GameType.Insurgency:
                    return 264;
                case GameType.Minecraft:
                    return 265;
                case GameType.Rust:
                    return 256;
                case GameType.Arma:
                case GameType.Arma2:
                case GameType.Arma3:
                    return 252;
                default:
                    return 28;
            }
        }

        public static int ForumIdForTempBans(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.CallOfDuty2:
                    return 68;
                case GameType.CallOfDuty4:
                    return 69;
                case GameType.CallOfDuty5:
                    return 70;
                case GameType.Insurgency:
                    return 169;
                case GameType.Minecraft:
                    return 144;
                case GameType.Rust:
                    return 260;
                case GameType.Arma:
                case GameType.Arma2:
                case GameType.Arma3:
                    return 259;
                default:
                    return 28;
            }
        }

        public static int ForumIdForBans(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.CallOfDuty2:
                    return 68;
                case GameType.CallOfDuty4:
                    return 69;
                case GameType.CallOfDuty5:
                    return 70;
                case GameType.Insurgency:
                    return 169;
                case GameType.Minecraft:
                    return 144;
                case GameType.Rust:
                    return 260;
                case GameType.Arma:
                case GameType.Arma2:
                case GameType.Arma3:
                    return 259;
                default:
                    return 28;
            }
        }
    }
}