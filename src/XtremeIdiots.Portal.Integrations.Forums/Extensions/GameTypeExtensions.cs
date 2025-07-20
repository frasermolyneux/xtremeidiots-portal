using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Integrations.Forums.Extensions;

public static class GameTypeExtensions
{
    public static int ForumIdForObservations(this GameType gameType)
    {
        return gameType switch
        {
            GameType.CallOfDuty2 => 58,
            GameType.CallOfDuty4 => 59,
            GameType.CallOfDuty5 => 60,
            GameType.Insurgency => 264,
            GameType.Minecraft => 265,
            GameType.Rust => 256,
            GameType.Arma or GameType.Arma2 or GameType.Arma3 => 252,
            GameType.Unknown => throw new NotImplementedException(),
            GameType.ArkSurvivalEvolved => throw new NotImplementedException(),
            GameType.Battlefield1 => throw new NotImplementedException(),
            GameType.Battlefield3 => throw new NotImplementedException(),
            GameType.Battlefield4 => throw new NotImplementedException(),
            GameType.Battlefield5 => throw new NotImplementedException(),
            GameType.BattlefieldBadCompany2 => throw new NotImplementedException(),
            GameType.CrysisWars => throw new NotImplementedException(),
            GameType.Left4Dead2 => throw new NotImplementedException(),
            GameType.PlayerUnknownsBattleground => throw new NotImplementedException(),
            GameType.RisingStormVietnam => throw new NotImplementedException(),
            GameType.WarThunder => throw new NotImplementedException(),
            GameType.WorldOfWarships => throw new NotImplementedException(),
            GameType.WorldWar3 => throw new NotImplementedException(),
            GameType.UnrealTournament2004 => throw new NotImplementedException(),
            _ => 28,
        };
    }

    public static int ForumIdForKicks(this GameType gameType)
    {
        return gameType switch
        {
            GameType.CallOfDuty2 => 58,
            GameType.CallOfDuty4 => 59,
            GameType.CallOfDuty5 => 60,
            GameType.Insurgency => 264,
            GameType.Minecraft => 265,
            GameType.Rust => 256,
            GameType.Arma or GameType.Arma2 or GameType.Arma3 => 252,
            GameType.Unknown => throw new NotImplementedException(),
            GameType.ArkSurvivalEvolved => throw new NotImplementedException(),
            GameType.Battlefield1 => throw new NotImplementedException(),
            GameType.Battlefield3 => throw new NotImplementedException(),
            GameType.Battlefield4 => throw new NotImplementedException(),
            GameType.Battlefield5 => throw new NotImplementedException(),
            GameType.BattlefieldBadCompany2 => throw new NotImplementedException(),
            GameType.CrysisWars => throw new NotImplementedException(),
            GameType.Left4Dead2 => throw new NotImplementedException(),
            GameType.PlayerUnknownsBattleground => throw new NotImplementedException(),
            GameType.RisingStormVietnam => throw new NotImplementedException(),
            GameType.WarThunder => throw new NotImplementedException(),
            GameType.WorldOfWarships => throw new NotImplementedException(),
            GameType.WorldWar3 => throw new NotImplementedException(),
            GameType.UnrealTournament2004 => throw new NotImplementedException(),
            _ => 28,
        };
    }

    public static int ForumIdForWarnings(this GameType gameType)
    {
        return gameType switch
        {
            GameType.CallOfDuty2 => 58,
            GameType.CallOfDuty4 => 59,
            GameType.CallOfDuty5 => 60,
            GameType.Insurgency => 264,
            GameType.Minecraft => 265,
            GameType.Rust => 256,
            GameType.Arma or GameType.Arma2 or GameType.Arma3 => 252,
            GameType.Unknown => throw new NotImplementedException(),
            GameType.ArkSurvivalEvolved => throw new NotImplementedException(),
            GameType.Battlefield1 => throw new NotImplementedException(),
            GameType.Battlefield3 => throw new NotImplementedException(),
            GameType.Battlefield4 => throw new NotImplementedException(),
            GameType.Battlefield5 => throw new NotImplementedException(),
            GameType.BattlefieldBadCompany2 => throw new NotImplementedException(),
            GameType.CrysisWars => throw new NotImplementedException(),
            GameType.Left4Dead2 => throw new NotImplementedException(),
            GameType.PlayerUnknownsBattleground => throw new NotImplementedException(),
            GameType.RisingStormVietnam => throw new NotImplementedException(),
            GameType.WarThunder => throw new NotImplementedException(),
            GameType.WorldOfWarships => throw new NotImplementedException(),
            GameType.WorldWar3 => throw new NotImplementedException(),
            GameType.UnrealTournament2004 => throw new NotImplementedException(),
            _ => 28,
        };
    }

    public static int ForumIdForTempBans(this GameType gameType)
    {
        return gameType switch
        {
            GameType.CallOfDuty2 => 68,
            GameType.CallOfDuty4 => 69,
            GameType.CallOfDuty5 => 70,
            GameType.Insurgency => 169,
            GameType.Minecraft => 144,
            GameType.Rust => 260,
            GameType.Arma or GameType.Arma2 or GameType.Arma3 => 259,
            GameType.Unknown => throw new NotImplementedException(),
            GameType.ArkSurvivalEvolved => throw new NotImplementedException(),
            GameType.Battlefield1 => throw new NotImplementedException(),
            GameType.Battlefield3 => throw new NotImplementedException(),
            GameType.Battlefield4 => throw new NotImplementedException(),
            GameType.Battlefield5 => throw new NotImplementedException(),
            GameType.BattlefieldBadCompany2 => throw new NotImplementedException(),
            GameType.CrysisWars => throw new NotImplementedException(),
            GameType.Left4Dead2 => throw new NotImplementedException(),
            GameType.PlayerUnknownsBattleground => throw new NotImplementedException(),
            GameType.RisingStormVietnam => throw new NotImplementedException(),
            GameType.WarThunder => throw new NotImplementedException(),
            GameType.WorldOfWarships => throw new NotImplementedException(),
            GameType.WorldWar3 => throw new NotImplementedException(),
            GameType.UnrealTournament2004 => throw new NotImplementedException(),
            _ => 28,
        };
    }

    public static int ForumIdForBans(this GameType gameType)
    {
        return gameType switch
        {
            GameType.CallOfDuty2 => 68,
            GameType.CallOfDuty4 => 69,
            GameType.CallOfDuty5 => 70,
            GameType.Insurgency => 169,
            GameType.Minecraft => 144,
            GameType.Rust => 260,
            GameType.Arma or GameType.Arma2 or GameType.Arma3 => 259,
            GameType.Unknown => throw new NotImplementedException(),
            GameType.ArkSurvivalEvolved => throw new NotImplementedException(),
            GameType.Battlefield1 => throw new NotImplementedException(),
            GameType.Battlefield3 => throw new NotImplementedException(),
            GameType.Battlefield4 => throw new NotImplementedException(),
            GameType.Battlefield5 => throw new NotImplementedException(),
            GameType.BattlefieldBadCompany2 => throw new NotImplementedException(),
            GameType.CrysisWars => throw new NotImplementedException(),
            GameType.Left4Dead2 => throw new NotImplementedException(),
            GameType.PlayerUnknownsBattleground => throw new NotImplementedException(),
            GameType.RisingStormVietnam => throw new NotImplementedException(),
            GameType.WarThunder => throw new NotImplementedException(),
            GameType.WorldOfWarships => throw new NotImplementedException(),
            GameType.WorldWar3 => throw new NotImplementedException(),
            GameType.UnrealTournament2004 => throw new NotImplementedException(),
            _ => 28,
        };
    }
}