using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using RestSharp;

using System.Net;

using XtremeIdiots.Portal.ServersApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.ServersApi.Abstractions.Models;

namespace XtremeIdiots.Portal.ServersApiClient.Api
{
    public class QueryApi : BaseApi, IQueryApi
    {
        public QueryApi(ILogger<QueryApi> logger, IOptions<ServersApiClientOptions> options, IServersApiTokenProvider serversApiTokenProvider) : base(logger, options, serversApiTokenProvider)
        {
        }

        public async Task<ServerQueryStatusResponseDto?> GetServerStatus(Guid gameServerId)
        {
            var request = await CreateRequest($"query/{gameServerId}/status", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<ServerQueryStatusResponseDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
