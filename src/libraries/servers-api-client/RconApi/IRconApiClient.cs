using XtremeIdiots.Portal.ServersApi.Abstractions.Models;

namespace XtremeIdiots.Portal.ServersApiClient.RconApi
{
    public interface IRconApiClient
    {
        Task<ServerRconStatusResponseDto?> GetServerStatus(Guid serverId);
    }
}
