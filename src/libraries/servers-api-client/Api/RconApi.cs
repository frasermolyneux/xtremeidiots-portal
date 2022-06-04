using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using RestSharp;

using XtremeIdiots.Portal.ServersApi.Abstractions.Models;
using XtremeIdiots.Portal.ServersApiClient.Interfaces;

namespace XtremeIdiots.Portal.ServersApiClient.Api
{
    public class RconApi : BaseApi, IRconApi
    {
        public RconApi(ILogger<RconApi> logger, IOptions<ServersApiClientOptions> options, IServersApiTokenProvider serversApiTokenProvider) : base(logger, options, serversApiTokenProvider)
        {
        }

        public async Task<ServerRconStatusResponseDto?> GetServerStatus(Guid serverId)
        {
            var request = await CreateRequest($"servers/rcon/{serverId}/status", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<ServerRconStatusResponseDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
