using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApiClient.PlayersApi
{
    public interface IPlayersApiClient
    {
        Task<PlayerDto?> GetPlayer(Guid playerId);
        Task<List<AliasDto>?> GetPlayerAliases(Guid playerId);
        Task<List<IpAddressDto>?> GetPlayerIpAddresses(Guid playerId);
        Task<List<RelatedPlayerDto>?> GetRelatedPlayers(Guid playerId, string ipAddress);
        Task<PlayerDto?> GetPlayerByGameType(GameType gameType, string guid);
        Task CreatePlayer(PlayerDto player);
        Task UpdatePlayer(PlayerDto player);
        Task<PlayersSearchResponseDto?> SearchPlayers(string gameType, string filterType, string filterString, int takeEntries, int skipEntries, string? order);
        Task<List<AdminActionDto>?> GetAdminActionsForPlayer(Guid playerId);
        Task<AdminActionDto?> CreateAdminActionForPlayer(AdminActionDto adminAction);
        Task<AdminActionDto?> UpdateAdminActionForPlayer(AdminActionDto adminAction);
    }
}