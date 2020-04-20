using System;
using System.Threading.Tasks;
using XI.CommonTypes;

namespace XI.Portal.Maps.Interfaces
{
    public interface IMapImageRepository
    {
        Task<Uri> GetMapImage(GameType gameType, string mapName);
    }
}