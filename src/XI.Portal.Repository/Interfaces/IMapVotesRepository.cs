using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Bus.Models;

namespace XI.Portal.Repository.Interfaces
{
    public interface IMapVotesRepository
    {
        Task UpdateMapVote(MapVote mapVote);
        Task RebuildIndex(IEnumerable<Tuple<GameType, string>> maps);
    }
}