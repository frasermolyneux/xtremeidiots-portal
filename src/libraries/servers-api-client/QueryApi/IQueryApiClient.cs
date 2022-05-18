using XtremeIdiots.Portal.ServersApi.Abstractions.Models;

namespace XtremeIdiots.Portal.ServersApiClient.QueryApi
{
    public interface IQueryApiClient
    {
        Task<ServerQueryStatusResponseDto?> GetServerStatus(Guid serverId);
    }
}
