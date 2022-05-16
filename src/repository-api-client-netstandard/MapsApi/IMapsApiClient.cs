using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Repository.Dtos;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.MapsApi
{
    public interface IMapsApiClient
    {
        Task<List<MapDto>> CreateMaps(List<MapDto> mapDtos);
        Task<MapsResponseDto> GetMaps(GameType? gameType, string[]? mapNames, string? filterString, int? skipEntries, int? takeEntries, MapsOrder? order);
        Task<List<MapDto>> UpdateMaps(List<MapDto> mapDtos);

        Task<MapDto> CreateMap(MapDto mapDto);
        Task<MapDto?> GetMap(Guid mapId);
        Task<MapDto?> GetMap(GameType gameType, string mapName);
        Task<MapDto> UpdateMap(MapDto mapDto);

        Task DeleteMap(Guid mapId);
        Task RebuildMapPopularity(string accessToken);

        Task UpsertMapVote(Guid mapId, Guid playerId, bool like, DateTime? overrideCreated = null);
        Task UpdateMapImage(Guid mapId, string filePath);
    }
}
