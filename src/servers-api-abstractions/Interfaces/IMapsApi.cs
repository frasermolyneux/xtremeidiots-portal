using XtremeIdiots.Portal.ServersApi.Abstractions.Models.Maps;

namespace XtremeIdiots.Portal.ServersApi.Abstractions.Interfaces
{
    public interface IMapsApi
    {
        Task<ApiResponseDto<ServerMapsCollectionDto>> GetServerMaps(Guid gameServerId);
        Task<ApiResponseDto> PushServerMap(Guid gameServerId, string mapName);
    }
}
