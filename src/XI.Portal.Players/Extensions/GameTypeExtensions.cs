namespace XI.Portal.Players.Extensions
{
    public static class GameTypeExtensions
    {
        public static int ForumIdForObservations(this string gameType)
        {
            switch (gameType)
            {
                case "CallOfDuty2":
                    return 58;
                case "CallOfDuty4":
                    return 59;
                case "CallOfDuty5":
                    return 60;
                case "Insurgency":
                    return 264;
                case "Minecraft":
                    return 265;
                case "Rust":
                    return 256;
                case "Arma":
                case "Arma2":
                case "Arma3":
                    return 252;
                default:
                    return 28;
            }
        }

        public static int ForumIdForKicks(this string gameType)
        {
            switch (gameType)
            {
                case "CallOfDuty2":
                    return 58;
                case "CallOfDuty4":
                    return 59;
                case "CallOfDuty5":
                    return 60;
                case "Insurgency":
                    return 264;
                case "Minecraft":
                    return 265;
                case "Rust":
                    return 256;
                case "Arma":
                case "Arma2":
                case "Arma3":
                    return 252;
                default:
                    return 28;
            }
        }

        public static int ForumIdForWarnings(this string gameType)
        {
            switch (gameType)
            {
                case "CallOfDuty2":
                    return 58;
                case "CallOfDuty4":
                    return 59;
                case "CallOfDuty5":
                    return 60;
                case "Insurgency":
                    return 264;
                case "Minecraft":
                    return 265;
                case "Rust":
                    return 256;
                case "Arma":
                case "Arma2":
                case "Arma3":
                    return 252;
                default:
                    return 28;
            }
        }

        public static int ForumIdForTempBans(this string gameType)
        {
            switch (gameType)
            {
                case "CallOfDuty2":
                    return 68;
                case "CallOfDuty4":
                    return 69;
                case "CallOfDuty5":
                    return 70;
                case "Insurgency":
                    return 169;
                case "Minecraft":
                    return 144;
                case "Rust":
                    return 260;
                case "Arma":
                case "Arma2":
                case "Arma3":
                    return 259;
                default:
                    return 28;
            }
        }

        public static int ForumIdForBans(this string gameType)
        {
            switch (gameType)
            {
                case "CallOfDuty2":
                    return 68;
                case "CallOfDuty4":
                    return 69;
                case "CallOfDuty5":
                    return 70;
                case "Insurgency":
                    return 169;
                case "Minecraft":
                    return 144;
                case "Rust":
                    return 260;
                case "Arma":
                case "Arma2":
                case "Arma3":
                    return 259;
                default:
                    return 28;
            }
        }
    }
}