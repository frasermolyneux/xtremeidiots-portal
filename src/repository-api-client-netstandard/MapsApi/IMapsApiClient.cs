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
        Task<List<MapDto>> CreateMaps(string accessToken, List<MapDto> mapDtos);
        Task<MapsResponseDto> GetMaps(string accessToken, GameType? gameType, string[]? mapNames, string? filterString, int? skipEntries, int? takeEntries, MapsOrder? order);
        Task<List<MapDto>> UpdateMaps(string accessToken, List<MapDto> mapDtos);

        Task<MapDto> CreateMap(string accessToken, MapDto mapDto);
        Task<MapDto?> GetMap(string accessToken, Guid mapId);
        Task<MapDto?> GetMap(string accessToken, GameType gameType, string mapName);
        Task<MapDto> UpdateMap(string accessToken, MapDto mapDto);

        Task DeleteMap(string accessToken, Guid mapId);
        Task RebuildMapPopularity(string accessToken);

        Task UpsertMapVote(string accessToken, Guid mapId, Guid playerId, bool like, DateTime? overrideCreated = null);
        Task UpdateMapImage(string accessToken, Guid mapId, string filePath);
    }
}
