using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using RestSharp;

using System.Net;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class DemosApi : BaseApi, IDemosApi
    {
        public DemosApi(ILogger<DemosApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<DemoDto?> CreateDemo(CreateDemoDto demoDto, string fileName, string filePath)
        {
            var request = await CreateRequest("demos", Method.Post);
            request.AddJsonBody(demoDto);

            var response = await ExecuteAsync(request);

            DemoDto? resultDemoDto = null;

            if (response.Content != null)
                resultDemoDto = JsonConvert.DeserializeObject<DemoDto>(response.Content);

            if (resultDemoDto == null)
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");

            var createFileRequest = await CreateRequest($"demos/{resultDemoDto.DemoId}/file", Method.Post);
            createFileRequest.AddFile(fileName, filePath);

            var createFileResponse = await ExecuteAsync(createFileRequest);

            if (createFileResponse.Content != null)
                return JsonConvert.DeserializeObject<DemoDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task DeleteDemo(Guid demoId)
        {
            var request = await CreateRequest($"demos/{demoId}", Method.Delete);
            await ExecuteAsync(request);
        }

        public async Task<DemoDto?> GetDemo(Guid demoId)
        {
            var request = await CreateRequest($"demos/{demoId}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<DemoDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<DemosSearchResponseDto?> SearchDemos(GameType[]? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, DemoOrder? order)
        {
            var request = await CreateRequest("demos", Method.Get);

            if (gameTypes != null)
                request.AddQueryParameter("gameTypes", string.Join(",", gameTypes));

            if (!string.IsNullOrWhiteSpace(userId))
                request.AddQueryParameter("userId", userId);

            if (!string.IsNullOrWhiteSpace(filterString))
                request.AddQueryParameter("filterString", filterString);

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (order != null)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<DemosSearchResponseDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
