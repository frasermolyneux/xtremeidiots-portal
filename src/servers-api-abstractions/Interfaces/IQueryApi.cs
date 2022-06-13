using XtremeIdiots.Portal.ServersApi.Abstractions.Models;

namespace XtremeIdiots.Portal.ServersApi.Abstractions.Interfaces
{
    public interface IQueryApi
    {
        Task<ServerQueryStatusResponseDto?> GetServerStatus(Guid gameServerId);
    }
}
