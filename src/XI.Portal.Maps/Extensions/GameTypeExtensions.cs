﻿using System;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Maps.Extensions
{
    public static class GameTypeExtensions
    {
        public static string ToRedirectShortName(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.Unknown:
                    break;
                case GameType.CallOfDuty2:
                    return "cod2";
                case GameType.CallOfDuty4:
                    return "cod4";
                case GameType.CallOfDuty5:
                    return "cod5";
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType));
            }

            return gameType.ToString();
        }

        public static string ToGameTrackerShortName(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.Unknown:
                    break;
                case GameType.CallOfDuty2:
                    return "cod2";
                case GameType.CallOfDuty4:
                    return "cod4";
                case GameType.CallOfDuty5:
                    return "codww";
                case GameType.Insurgency:
                    return "ins";
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
                    return "left4dead2";
                case GameType.Minecraft:
                    break;
                case GameType.PlayerUnknownsBattleground:
                    break;
                case GameType.RisingStormVietnam:
                    break;
                case GameType.Rust:
                    return "rust";
                case GameType.WarThunder:
                    break;
                case GameType.WorldOfWarships:
                    break;
                case GameType.WorldWar3:
                    break;
                case GameType.UnrealTournament2004:
                    break;
            }

            return gameType.ToString();
        }
    }
}