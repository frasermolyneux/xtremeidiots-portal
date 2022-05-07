using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.PlayersApi
{
    public interface IPlayersApiClient
    {
        Task<PlayerDto?> GetPlayer(string accessToken, Guid id);
        Task<List<AliasDto>> GetPlayerAliases(string accessToken, Guid id);
        Task<List<IpAddressDto>> GetPlayerIpAddresses(string accessToken, Guid id);
        Task<List<RelatedPlayerDto>> GetRelatedPlayers(string accessToken, Guid id, string ipAddress);
        Task<PlayerDto?> GetPlayerByGameType(string accessToken, string gameType, string guid);
        Task CreatePlayer(string accessToken, PlayerDto player);
        Task UpdatePlayer(string accessToken, PlayerDto player);
        Task<PlayersSearchResponseDto> SearchPlayers(string accessToken, string gameType, string filterType, string filterString, int takeEntries, int skipEntries, string? order);
        Task<List<AdminActionDto>> GetAdminActionsForPlayer(string accessToken, Guid playerId);
        Task<AdminActionDto> CreateAdminActionForPlayer(string accessToken, AdminActionDto adminAction);
        Task<AdminActionDto> UpdateAdminActionForPlayer(string accessToken, AdminActionDto adminAction);
    }
}