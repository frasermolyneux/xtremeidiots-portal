using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DemosRepositoryApi
{
    public class DemosApiClient : BaseApiClient, IDemosApiClient
    {
        public DemosApiClient(ILogger logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {
        }

        public async Task<DemoDto> CreateDemo(string accessToken, DemoDto demoDto)
        {
            var request = CreateRequest("repository/demos", Method.POST, accessToken);
            request.AddJsonBody(demoDto);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<DemoDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task DeleteDemo(string accessToken, Guid demoId)
        {
            var request = CreateRequest($"repository/demos/{demoId}", Method.DELETE, accessToken);
            await ExecuteAsync(request);
        }

        public async Task<DemoDto> GetDemo(string accessToken, Guid demoId)
        {
            var request = CreateRequest($"repository/demos/{demoId}", Method.GET, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<DemoDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<DemosSearchResponseDto> SearchDemos(string accessToken, string[]? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, string? order)
        {
            var request = CreateRequest("repository/demos", Method.GET, accessToken);

            if (gameTypes != null)
                request.AddQueryParameter("gameTypes", string.Join(",", gameTypes));

            if (!string.IsNullOrWhiteSpace(userId))
                request.AddQueryParameter("userId", userId);

            if (!string.IsNullOrWhiteSpace(filterString))
                request.AddQueryParameter("filterString", filterString);

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (!string.IsNullOrWhiteSpace(order))
                request.AddQueryParameter("order", order);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<DemosSearchResponseDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
