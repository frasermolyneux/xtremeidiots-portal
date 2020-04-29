using XI.CommonTypes;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Extensions
{
    public static class GameServerDtoExtensions
    {
        public static GameServerDto ForGameType(this GameServerDto gameServerDto, GameType gameType)
        {
            gameServerDto.GameType = gameType;
            return gameServerDto;
        }
    }
}