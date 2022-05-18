using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.MapsApi
{
    public interface IMapsApiClient
    {
        Task<List<MapDto>?> CreateMaps(List<MapDto> mapDtos);
        Task<MapsResponseDto?> GetMaps(GameType? gameType, string[]? mapNames, string? filterString, int? skipEntries, int? takeEntries, MapsOrder? order);
        Task<List<MapDto>?> UpdateMaps(List<MapDto> mapDtos);

        Task<MapDto?> CreateMap(MapDto mapDto);
        Task<MapDto?> GetMap(Guid mapId);
        Task<MapDto?> GetMap(GameType gameType, string mapName);
        Task<MapDto?> UpdateMap(MapDto mapDto);

        Task DeleteMap(Guid mapId);
        Task RebuildMapPopularity();

        Task UpsertMapVote(Guid mapId, Guid playerId, bool like, DateTime? overrideCreated = null);
        Task UpdateMapImage(Guid mapId, string filePath);
    }
}
