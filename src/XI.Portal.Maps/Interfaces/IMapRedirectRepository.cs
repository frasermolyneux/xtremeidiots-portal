using System.Collections.Generic;
using XI.Portal.Maps.Models;

namespace XI.Portal.Maps.Interfaces
{
    public interface IMapRedirectRepository
    {
        List<MapRedirectEntry> GetMapEntriesForGame(string game);
    }
}