using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Extensions
{
    public static class BanFileMonitorDtoExtensions
    {
        public static BanFileMonitorDto WithServerDto(this BanFileMonitorDto banFileMonitorDto, GameServerDto gameServerDto)
        {
            banFileMonitorDto.GameServer = gameServerDto;
            banFileMonitorDto.ServerId = gameServerDto.ServerId;

            return banFileMonitorDto;
        }
    }
}