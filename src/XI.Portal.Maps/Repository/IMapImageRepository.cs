using System;
using System.Threading.Tasks;
using XI.CommonTypes;

namespace XI.Portal.Maps.Repository
{
    public interface IMapImageRepository
    {
        Task<Uri> GetMapImage(GameType gameType, string mapName);
    }
}