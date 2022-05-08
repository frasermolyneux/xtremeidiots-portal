using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Repository.CloudEntities;
using XI.Portal.Repository.Dtos;
using XI.Portal.Repository.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Repository.Interfaces
{
    public interface IMapsRepository
    {
        Task InsertOrMergeMap(LegacyMapDto mapDto);
        Task InsertOrMergeMapVote(LegacyMapVoteDto mapVoteDto);
        Task<LegacyMapDto> GetMap(GameType gameType, string mapName);
        Task<int> GetMapsCount(MapsQueryOptions queryOptions);
        Task<List<LegacyMapDto>> GetMaps(MapsQueryOptions queryOptions);
        Task RebuildMapVotes();
        Task<List<MapVoteCloudEntity>> GetMapVotes();
    }
}