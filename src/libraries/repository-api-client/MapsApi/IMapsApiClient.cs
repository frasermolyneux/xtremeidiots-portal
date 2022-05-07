using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.MapsApi
{
    public interface IMapsApiClient
    {
        Task<List<MapDto>> CreateMaps(string accessToken, List<MapDto> mapDtos);
        Task<MapsResponseDto> GetMaps(string accessToken, GameType? gameType, string[]? mapNames, string? filterString, int? skipEntries, int? takeEntries, MapsOrder? order);
        Task<List<MapDto>> UpdateMaps(string accessToken, List<MapDto> mapDtos);

        Task<MapDto> CreateMap(string accessToken, MapDto mapDto);
        Task<MapDto?> GetMap(string accessToken, Guid mapId);
        Task<MapDto> UpdateMap(string accessToken, MapDto mapDto);

        Task DeleteMap(string accessToken, Guid mapId);
    }
}
