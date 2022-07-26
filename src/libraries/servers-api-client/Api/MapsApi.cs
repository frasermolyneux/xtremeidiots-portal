
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.ServersApi.Abstractions;
using XtremeIdiots.Portal.ServersApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.ServersApi.Abstractions.Models.Maps;
using XtremeIdiots.Portal.ServersApiClient.Extensions;

namespace XtremeIdiots.Portal.ServersApiClient.Api
{
    public class MapsApi : BaseApi, IMapsApi
    {
        public MapsApi(ILogger<MapsApi> logger, IOptions<ServersApiClientOptions> options, IServersApiTokenProvider serversApiTokenProvider) : base(logger, options, serversApiTokenProvider)
        {
        }

        public async Task<ApiResponseDto<ServerMapsCollectionDto>> GetServerMaps(Guid gameServerId)
        {
            var request = await CreateRequest($"maps/{gameServerId}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<ServerMapsCollectionDto>();
        }

        public async Task<ApiResponseDto> PushServerMap(Guid gameServerId, string mapName)
        {
            var request = await CreateRequest($"maps/{gameServerId}/{mapName}", Method.Post);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }
    }
}
