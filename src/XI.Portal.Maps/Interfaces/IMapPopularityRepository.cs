using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Maps.Dto;

namespace XI.Portal.Maps.Interfaces
{
    public interface IMapPopularityRepository
    {
        Task<MapPopularityDto> GetMapPopularity(GameType gameType, string mapName);
        Task UpdateMapPopularity(MapPopularityDto mapPopularityDto);
    }
}