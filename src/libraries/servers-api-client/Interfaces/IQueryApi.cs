using XtremeIdiots.Portal.ServersApi.Abstractions.Models;

namespace XtremeIdiots.Portal.ServersApiClient.Interfaces
{
    public interface IQueryApi
    {
        Task<ServerQueryStatusResponseDto?> GetServerStatus(Guid serverId);
    }
}
