using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.MapsApi
{
    public class MapsApiClient : BaseApiClient, IMapsApiClient
    {
        public MapsApiClient(ILogger<MapsApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {
        }

        public Task<MapDto> CreateMap(string accessToken, MapDto mapDto)
        {
            throw new NotImplementedException();
        }

        public Task<List<MapDto>> CreateMaps(string accessToken, List<MapDto> mapDtos)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMap(string accessToken, Guid mapId)
        {
            throw new NotImplementedException();
        }

        public Task<MapDto> GetMap(string accessToken, Guid mapId)
        {
            throw new NotImplementedException();
        }

        public Task<MapsResponseDto> GetMaps(string accessToken, GameType? gameType, string[] mapNames, string filterString, int skipEntries, int takeEntries, MapsOrder? order)
        {
            throw new NotImplementedException();
        }

        public Task<MapDto> UpdateMap(string accessToken, MapDto mapDto)
        {
            throw new NotImplementedException();
        }

        public Task<List<MapDto>> UpdateMaps(string accessToken, List<MapDto> mapDtos)
        {
            throw new NotImplementedException();
        }
    }
}
