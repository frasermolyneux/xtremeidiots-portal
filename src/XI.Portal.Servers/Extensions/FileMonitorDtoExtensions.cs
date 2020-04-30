using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Extensions
{
    public static class FileMonitorDtoExtensions
    {
        public static FileMonitorDto WithServerDto(this FileMonitorDto fileMonitorDto, GameServerDto gameServerDto)
        {
            fileMonitorDto.GameServer = gameServerDto;
            fileMonitorDto.ServerId = gameServerDto.ServerId;

            return fileMonitorDto;
        }
    }
}