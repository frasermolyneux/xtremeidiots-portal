using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Maps.Dto;

namespace XI.Portal.Maps.Interfaces
{
    public interface ILegacyMapPopularityRepository
    {
        Task<LegacyMapPopularityDto> GetMapPopularity(GameType gameType, string mapName);
        Task UpdateMapPopularity(LegacyMapPopularityDto legacyMapPopularityDto);
    }
}