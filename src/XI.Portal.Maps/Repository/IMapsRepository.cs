using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Maps.Models;

namespace XI.Portal.Maps.Repository
{
    public interface IMapsRepository
    {
        Task<int> GetMapListCount(MapsFilterModel filterModel);
        Task<List<MapsListEntryViewModel>> GetMapList(MapsFilterModel filterModel);
    }
}