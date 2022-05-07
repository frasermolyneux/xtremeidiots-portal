using System;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Maps.Interfaces
{
    public interface IMapImageRepository
    {
        Task<Uri> GetMapImage(GameType gameType, string mapName);
    }
}