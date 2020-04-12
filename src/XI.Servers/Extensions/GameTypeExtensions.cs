using System;
using XI.CommonTypes;

namespace XI.Servers.Extensions
{
    public static class GameTypeExtensions
    {
        public static GameProtocol Protocol(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.CallOfDuty2:
                case GameType.CallOfDuty4:
                case GameType.CallOfDuty5:
                    return GameProtocol.Quake3;
                case GameType.Insurgency:
                case GameType.Rust:
                case GameType.Left4Dead2:
                    return GameProtocol.Source;
                case GameType.UnrealTournament2004:
                    return GameProtocol.GameSpy;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), gameType, null);
            }
        }
    }
}