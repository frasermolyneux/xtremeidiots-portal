using XtremeIdiots.Portal.ServersApi.Abstractions.Models;

namespace XtremeIdiots.Portal.ServersApiClient.Interfaces
{
    public interface IRconApi
    {
        Task<ServerRconStatusResponseDto?> GetServerStatus(Guid serverId);
    }
}
