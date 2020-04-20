using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Maps.Dto;
using XI.Portal.Maps.Models;

namespace XI.Portal.Maps.Repository
{
    public interface IMapsRepository
    {
        Task<int> GetMapListCount(MapsFilterModel filterModel);
        Task<List<MapsListEntryViewModel>> GetMapList(MapsFilterModel filterModel);
        Task<MapDto> GetMap(GameType gameType, string mapName);
        Task<List<MapRotationDto>> GetMapRotation(Guid serverId);
    }
}