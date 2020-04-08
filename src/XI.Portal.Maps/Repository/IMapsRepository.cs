using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Data.Legacy.CommonTypes;
using XI.Portal.Maps.Models;

namespace XI.Portal.Maps.Repository
{
    public interface IMapsRepository
    {
        Task<int> GetMapListCount(MapsFilterModel filterModel);
        Task<List<MapsListEntryViewModel>> GetMapList(MapsFilterModel filterModel);
        Task<byte[]> GetFullRotationArchive(Guid id);
        Task<byte[]> GetMapImage(GameType gameType, string mapName);
    }
}