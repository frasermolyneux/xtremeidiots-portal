using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class DemosApi : BaseApi, IDemosApi
    {
        public DemosApi(ILogger<DemosApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<ApiResponseDto<DemoDto>> GetDemo(Guid demoId)
        {
            var request = await CreateRequest($"demos/{demoId}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<DemoDto>();
        }

        public async Task<ApiResponseDto<DemosCollectionDto>> GetDemos(GameType[]? gameTypes, string? userId, string? filterString, int skipEntries, int takeEntries, DemoOrder? order)
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

            if (order.HasValue)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<DemosCollectionDto>();
        }

        public async Task<ApiResponseDto<DemoDto>> CreateDemo(CreateDemoDto createDemoDto)
        {
            var request = await CreateRequest("demos", Method.Post);
            request.AddJsonBody(createDemoDto);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<DemoDto>();
        }

        public async Task<ApiResponseDto> SetDemoFile(Guid demoId, string fileName, string filePath)
        {
            var request = await CreateRequest($"demos/{demoId}/file", Method.Post);
            request.AddFile(fileName, filePath);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> DeleteDemo(Guid demoId)
        {
            var request = await CreateRequest($"demos/{demoId}", Method.Delete);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }
    }
}
