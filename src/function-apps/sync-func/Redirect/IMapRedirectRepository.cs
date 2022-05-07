using System.Collections.Generic;
using XtremeIdiots.Portal.SyncFunc.Models;

namespace XtremeIdiots.Portal.SyncFunc.Redirect
{
    public interface IMapRedirectRepository
    {
        List<MapRedirectEntry> GetMapEntriesForGame(string game);
    }
}