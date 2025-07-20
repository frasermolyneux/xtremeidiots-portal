using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Integrations.Forums.Extensions;

/// <summary>
/// Extension methods for mapping GameType to specific forum IDs in the Invision Community integration
/// </summary>
public static class GameTypeExtensions
{
    /// <summary>
    /// Gets the forum ID for posting observation-type admin actions for the specified game type
    /// </summary>
    /// <param name="gameType">The game type to get the forum ID for</param>
    /// <returns>The forum ID for observations, or 28 as default</returns>
    /// <exception cref="NotImplementedException">Thrown for unsupported game types that haven't been implemented yet</exception>
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

    /// <summary>
    /// Gets the forum ID for posting kick-type admin actions for the specified game type
    /// </summary>
    /// <param name="gameType">The game type to get the forum ID for</param>
    /// <returns>The forum ID for kicks, or 28 as default</returns>
    /// <exception cref="NotImplementedException">Thrown for unsupported game types that haven't been implemented yet</exception>
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

    /// <summary>
    /// Gets the forum ID for posting warning-type admin actions for the specified game type
    /// </summary>
    /// <param name="gameType">The game type to get the forum ID for</param>
    /// <returns>The forum ID for warnings, or 28 as default</returns>
    /// <exception cref="NotImplementedException">Thrown for unsupported game types that haven't been implemented yet</exception>
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

    /// <summary>
    /// Gets the forum ID for posting temporary ban-type admin actions for the specified game type
    /// </summary>
    /// <param name="gameType">The game type to get the forum ID for</param>
    /// <returns>The forum ID for temporary bans, or 28 as default</returns>
    /// <exception cref="NotImplementedException">Thrown for unsupported game types that haven't been implemented yet</exception>
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

    /// <summary>
    /// Gets the forum ID for posting permanent ban-type admin actions for the specified game type
    /// </summary>
    /// <param name="gameType">The game type to get the forum ID for</param>
    /// <returns>The forum ID for permanent bans, or 28 as default</returns>
    /// <exception cref="NotImplementedException">Thrown for unsupported game types that haven't been implemented yet</exception>
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