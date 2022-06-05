using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IMapsApi
    {
        Task<ApiResponseDto<MapDto>> GetMap(Guid mapId);
        Task<ApiResponseDto<MapDto>> GetMap(GameType gameType, string mapName);
        Task<ApiResponseDto<MapsCollectionDto>> GetMaps(GameType? gameType, string[]? mapNames, MapsFilter? filter, string? filterString, int skipEntries, int takeEntries, MapsOrder? order);

        Task<ApiResponseDto> CreateMap(CreateMapDto createMapDto);
        Task<ApiResponseDto> CreateMaps(List<CreateMapDto> createMapDtos);

        Task<ApiResponseDto> UpdateMap(EditMapDto editMapDto);
        Task<ApiResponseDto> UpdateMaps(List<EditMapDto> editMapDtos);

        Task<ApiResponseDto> DeleteMap(Guid mapId);
        Task<ApiResponseDto> RebuildMapPopularity();

        Task<ApiResponseDto> UpsertMapVote(Guid mapId, Guid playerId, bool like);
        Task<ApiResponseDto> UpdateMapImage(Guid mapId, string filePath);
    }
}
