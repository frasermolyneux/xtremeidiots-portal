using XtremeIdiots.Portal.ServersApi.Abstractions.Models;

namespace XtremeIdiots.Portal.ServersApi.Abstractions.Interfaces
{
    public interface IRconApi
    {
        Task<ServerRconStatusResponseDto?> GetServerStatus(Guid gameServerId);
    }
}
