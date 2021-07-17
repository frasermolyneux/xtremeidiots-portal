using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Maps.Dto;
using XI.Portal.Maps.Models;

namespace XI.Portal.Maps.Interfaces
{
    public interface ILegacyMapsRepository
    {
        Task<int> GetMapsCount(LegacyMapsFilterModel filterModel);
        Task<List<LegacyMapDto>> GetMaps(LegacyMapsFilterModel filterModel);
        Task<LegacyMapDto> GetMap(GameType gameType, string mapName);
        Task CreateMap(LegacyMapDto legacyMapDto);
        Task UpdateMap(LegacyMapDto legacyMapDto);
        Task DeleteMap(Guid mapId);
    }
}