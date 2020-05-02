using System;

namespace XI.CommonTypes.Extensions
{
    public static class GameTypeExtensions
    {
        public static string DisplayName(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.Unknown:
                    break;
                case GameType.CallOfDuty2:
                    return "Call of Duty 2";
                case GameType.CallOfDuty4:
                    return "Call of Duty 4";
                case GameType.CallOfDuty5:
                    return "Call of Duty 5";
                case GameType.Insurgency:
                    break;
                case GameType.ArkSurvivalEvolved:
                    break;
                case GameType.Battlefield1:
                    break;
                case GameType.Battlefield3:
                    break;
                case GameType.Battlefield4:
                    break;
                case GameType.Battlefield5:
                    break;
                case GameType.BattlefieldBadCompany2:
                    break;
                case GameType.CrysisWars:
                    break;
                case GameType.Left4Dead2:
                    break;
                case GameType.Minecraft:
                    break;
                case GameType.PlayerUnknownsBattleground:
                    break;
                case GameType.RisingStormVietnam:
                    break;
                case GameType.Rust:
                    break;
                case GameType.WarThunder:
                    break;
                case GameType.WorldOfWarships:
                    break;
                case GameType.WorldWar3:
                    break;
                case GameType.UnrealTournament2004:
                    break;
                case GameType.Arma:
                    break;
                case GameType.Arma2:
                    break;
                case GameType.Arma3:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null);
            }

            return gameType.ToString();
        }

        public static string ShortDisplayName(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.Unknown:
                    break;
                case GameType.CallOfDuty2:
                    return "COD2";
                case GameType.CallOfDuty4:
                    return "COD4";
                case GameType.CallOfDuty5:
                    return "COD5";
                case GameType.Insurgency:
                    break;
                case GameType.ArkSurvivalEvolved:
                    break;
                case GameType.Battlefield1:
                    break;
                case GameType.Battlefield3:
                    break;
                case GameType.Battlefield4:
                    break;
                case GameType.Battlefield5:
                    break;
                case GameType.BattlefieldBadCompany2:
                    break;
                case GameType.CrysisWars:
                    break;
                case GameType.Left4Dead2:
                    break;
                case GameType.Minecraft:
                    break;
                case GameType.PlayerUnknownsBattleground:
                    break;
                case GameType.RisingStormVietnam:
                    break;
                case GameType.Rust:
                    break;
                case GameType.WarThunder:
                    break;
                case GameType.WorldOfWarships:
                    break;
                case GameType.WorldWar3:
                    break;
                case GameType.UnrealTournament2004:
                    break;
                case GameType.Arma:
                    break;
                case GameType.Arma2:
                    break;
                case GameType.Arma3:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null);
            }

            return gameType.ToString();
        }
    }
}