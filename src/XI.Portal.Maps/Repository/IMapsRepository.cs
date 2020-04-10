using System.Collections.Generic;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Maps.Models;

namespace XI.Portal.Maps.Repository
{
    public interface IMapsRepository
    {
        Task<IMapDto> GetGameMap(GameType gameType, string mapName);
        Task UpdateMap(IMapDto mapDto);
        Task<int> GetMapListCount(MapsFilterModel filterModel);
        Task<List<MapsListEntryViewModel>> GetMapList(MapsFilterModel filterModel);
    }
}