using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Extensions
{
    public static class RconMonitorDtoExtensions
    {
        public static RconMonitorDto WithServerDto(this RconMonitorDto rconMonitorDto, GameServerDto gameServerDto)
        {
            rconMonitorDto.GameServer = gameServerDto;
            rconMonitorDto.ServerId = gameServerDto.ServerId;

            return rconMonitorDto;
        }
    }
}