﻿using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Repository.Dtos;
using XI.Portal.Repository.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Repository.Interfaces
{
    public interface IMapsRepository
    {
        Task InsertOrMergeMap(MapDto mapDto);
        Task InsertOrMergeMapVote(MapVoteDto mapVoteDto);
        Task<MapDto> GetMap(GameType gameType, string mapName);
        Task<int> GetMapsCount(MapsQueryOptions queryOptions);
        Task<List<MapDto>> GetMaps(MapsQueryOptions queryOptions);
        Task RebuildMapVotes();
    }
}