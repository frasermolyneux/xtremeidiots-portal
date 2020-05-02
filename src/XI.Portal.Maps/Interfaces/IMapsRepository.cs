using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Maps.Dto;
using XI.Portal.Maps.Models;

namespace XI.Portal.Maps.Interfaces
{
    public interface IMapsRepository
    {
        Task<int> GetMapListCount(MapsFilterModel filterModel);
        Task<List<MapDto>> GetMapList(MapsFilterModel filterModel);
        Task<MapDto> GetMap(GameType gameType, string mapName);
        Task<List<MapRotationDto>> GetMapRotation(Guid serverId);
        Task CreateMap(MapDto mapDto);
        Task UpdateMap(MapDto mapDto);
        Task DeleteMap(Guid mapId);
    }
}